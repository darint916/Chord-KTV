using System.Globalization;
using System.Text.RegularExpressions;
using ChordKTV.Data.Api.SongData;
using ChordKTV.Data.Api.QuizData;
using ChordKTV.Models.Quiz;
using ChordKTV.Models.SongData;
using ChordKTV.Services.Api;

namespace ChordKTV.Services.Service;

public class QuizService : IQuizService
{
    private readonly ISongRepo _songRepo;
    private readonly IChatGptService _chatGptService;
    private readonly IQuizRepo _quizRepo;
    private readonly ILogger<QuizService> _logger;

    public QuizService(ISongRepo songRepo, IChatGptService chatGptService, IQuizRepo quizRepo, ILogger<QuizService> logger)
    {
        _songRepo = songRepo;
        _chatGptService = chatGptService;
        _quizRepo = quizRepo;
        _logger = logger;
    }

    private bool HasDuplicateOptions(Quiz quiz)
    {
        foreach (QuizQuestion question in quiz.Questions)
        {
            HashSet<string> uniqueOptions = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            foreach (QuizOption option in question.Options)
            {
                if (!uniqueOptions.Add(option.Text))
                {
                    _logger.LogWarning("Found duplicate option in question {QuestionNumber}: {Option}",
                        question.QuestionNumber, option.Text);
                    return true;
                }
            }
        }
        return false;
    }

    public async Task<Quiz> GenerateQuizAsync(Guid songId, bool useCachedQuiz, int difficulty, int numQuestions)
    {
        // Clamp difficulty between 1 and 5
        difficulty = Math.Clamp(difficulty, 1, 5);

        // Clamp number of questions between 1 and 20
        numQuestions = Math.Clamp(numQuestions, 1, 20);

        // Try to use a cached quiz if requested
        if (useCachedQuiz)
        {
            Quiz? cachedQuiz = await _quizRepo.GetLatestQuizAsync(songId, difficulty);
            if (cachedQuiz != null)
            {

                if (!HasDuplicateOptions(cachedQuiz))
                {
                    return cachedQuiz;
                }

            }
        }

        // Retrieve the song by songId and verify its lyrics
        Song? song = await _songRepo.GetSongByIdAsync(songId) ??
            throw new InvalidOperationException($"Song with ID {songId} not found in database.");

        if (string.IsNullOrWhiteSpace(song.LrcLyrics))
        {
            throw new InvalidOperationException("Song lyrics not available.");
        }

        // 3 is the maximum number of retries for quiz generation.
        // It is already unlikely that the first attempt will have duplicate options.
        // If we hit more than 3 retries, it means there is a problem with the input lyrics or the LLM. We should stop here to avoid an infinite loop.
        const int maxRetries = 3;
        Quiz quiz;
        int retryCount = 0;

        do
        {
            if (retryCount > 0)
            {
                _logger.LogWarning("Retrying quiz generation due to duplicate options. Attempt {RetryCount} of {MaxRetries}",
                    retryCount + 1, maxRetries);
            }

            // Generate a quiz by calling ChatGPT
            quiz = await _chatGptService.GenerateRomanizationQuizAsync(song.LrcLyrics, difficulty, numQuestions, songId);

            _logger.LogDebug("Generated new quiz with ID: {QuizId}, updated timestamp to: {Timestamp}",
                quiz.Id, quiz.Timestamp);

            retryCount++;

            if (!HasDuplicateOptions(quiz))
            {
                break;
            }

            if (retryCount >= maxRetries)
            {
                throw new InvalidOperationException("Failed to generate quiz without duplicate options after maximum retries.");
            }
        } while (true);

        await _quizRepo.AddAsync(quiz);

        return quiz;
    }

    /// Generate an "audio" quiz: pick timestamped lines from the LRC,
    /// ask user to identify the lyric at that time, with plausible distractors.
    public async Task<Quiz> GenerateAudioQuizAsync(
        Guid songId,
        bool useCachedQuiz,
        int difficulty,
        int numQuestions)
    {
        // 1) clamp inputs
        difficulty = Math.Clamp(difficulty, 1, 5);
        numQuestions = Math.Clamp(numQuestions, 1, 20);

        // 2) optional cache‐lookup (stubbed for now)
        if (useCachedQuiz)
        {
            // TODO: var cached = await _quizRepo.GetLatestAudioQuizAsync(songId, difficulty);
            // if (cached != null) return cached;
        }

        // 3) load song & ensure LRC exists
        Song song = await _songRepo.GetSongByIdAsync(songId)
                   ?? throw new InvalidOperationException($"Song with ID {songId} not found.");
        if (string.IsNullOrWhiteSpace(song.LrcLyrics))
        {
            throw new InvalidOperationException("Song LRC lyrics not available.");
        }

        // 4) parse LRC into (start, end, text) tuples
        List<(TimeSpan start, TimeSpan end, string text)> timestampedLines = ParseLrcLines(song.LrcLyrics);
        if (timestampedLines.Count == 0)
        {
            throw new InvalidOperationException("No timestamped lines found in LRC.");
        }

        // 5) pick random lines
        var rng = new Random();
        var selected = timestampedLines
            .OrderBy(_ => rng.Next())
            .Take(numQuestions)
            .ToList();

        // 6) build questions
        List<QuizQuestion> questions = new List<QuizQuestion>();
        int qNum = 1;

        foreach ((TimeSpan start, TimeSpan end, string lyric) in selected)
        {
            // TODO: implement this in IChatGptService when ready
            List<string> distractors = await _chatGptService
                .GenerateAudioQuizDistractorsAsync(lyric, difficulty);

            // assemble options
            List<QuizOption> options = new()
            {
                new QuizOption { Text = lyric,   IsCorrect = true  }
            };
            options.AddRange(
                distractors.Select(d => new QuizOption { Text = d, IsCorrect = false }));

            // shuffle & log duplicates
            options = options.OrderBy(_ => rng.Next()).ToList();
            if (options.Select(o => o.Text)
                       .Distinct(StringComparer.OrdinalIgnoreCase)
                       .Count() != options.Count)
            {
                _logger.LogWarning(
                    "Duplicate options generated for audio question #{QuestionNumber}", qNum);
            }

            // 2) assign OrderIndex
            for (int i = 0; i < options.Count; i++)
            {
                options[i].OrderIndex = i;
            }

            questions.Add(new QuizQuestion
            {
                QuestionNumber = qNum,
                StartTimestamp = start,
                EndTimestamp = end,
                Options = options
            });
            qNum++;
        }

        // 7) persist & return
        var quiz = new Quiz
        {
            Id = Guid.NewGuid(),
            Timestamp = DateTime.UtcNow,
            SongId = songId,
            Difficulty = difficulty,
            Questions = questions,
        };
        await _quizRepo.AddAsync(quiz);

        return quiz;
    }

    /// Parse raw LRC into a list of (start, end, text).
    /// Uses next‐line start as end
    private static readonly string[] _lineSeparators = ["\r\n", "\n"];

    private static List<(TimeSpan Start, TimeSpan End, string Text)> ParseLrcLines(string lrc)
    {
        string[] lines = lrc
            .Split(_lineSeparators, StringSplitOptions.RemoveEmptyEntries);
        Regex pattern = new(@"\[(\d+):(\d+(?:\.\d+)?)\]");
        List<(TimeSpan start, string text)> temp = new();

        foreach (string raw in lines)
        {
            Match m = pattern.Match(raw);
            if (!m.Success)
            { continue; }

            int mins = int.Parse(m.Groups[1].Value, CultureInfo.InvariantCulture);
            double secs = double.Parse(m.Groups[2].Value, CultureInfo.InvariantCulture);
            TimeSpan start = TimeSpan.FromSeconds((mins * 60) + secs);

            string text = pattern.Replace(raw, "").Trim();
            if (text.Length == 0)
            { continue; }
            temp.Add((start, text));
        }

        // sort & infer end‐times
        temp.Sort((a, b) => a.start.CompareTo(b.start));
        List<(TimeSpan start, TimeSpan end, string text)> result = new();

        for (int i = 0; i < temp.Count; i++)
        {
            (TimeSpan start, string text) = temp[i];
            TimeSpan end = (i < temp.Count - 1)
                ? temp[i + 1].start
                : start.Add(TimeSpan.FromSeconds(5));
            result.Add((start, end, text));
        }

        return result;
    }
}
