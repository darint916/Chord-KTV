using System;
using System.Threading.Tasks;
using ChordKTV.Data.Api.SongData;
using ChordKTV.Data.Api.QuizData;
using ChordKTV.Models.Quiz;
using ChordKTV.Services.Api;
using Microsoft.Extensions.Logging;

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

        _logger.LogDebug("Generating quiz for SongId={SongId}, Difficulty={Difficulty}, UseCached={UseCached}",
            songId, difficulty, useCachedQuiz);

        // Try to use a cached quiz if requested
        if (useCachedQuiz)
        {
            _logger.LogDebug("Attempting to retrieve cached quiz");
            Quiz? cachedQuiz = await _quizRepo.GetLatestQuizAsync(songId, difficulty);
            if (cachedQuiz != null)
            {
                _logger.LogDebug("Found cached quiz with ID: {QuizId}, Timestamp: {Timestamp}",
                    cachedQuiz.Id, cachedQuiz.Timestamp);

                if (!HasDuplicateOptions(cachedQuiz))
                {
                    return cachedQuiz;
                }

                _logger.LogDebug("Cached quiz has duplicate options, generating new quiz");
            }
            else
            {
                _logger.LogDebug("No cached quiz found, generating new quiz");
            }
        }

        // Retrieve the song by songId and verify its lyrics
        Models.SongData.Song? song = await _songRepo.GetSongByIdAsync(songId) ??
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

        _logger.LogDebug("Successfully saved quiz with ID: {QuizId}, Timestamp: {Timestamp}",
            quiz.Id, quiz.Timestamp);

        return quiz;
    }
}
