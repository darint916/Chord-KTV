using System;
using System.Text.Json;
using System.Threading.Tasks;
using ChordKTV.Data.Api.SongData;
using ChordKTV.Data.Api.QuizData;
using ChordKTV.Dtos.Quiz;
using ChordKTV.Models.Quiz;
using ChordKTV.Services.Api;

namespace ChordKTV.Services.Service
{
    public class QuizService : IQuizService
    {
        private readonly ISongRepo _songRepo;
        private readonly IChatGptService _chatGptService;
        private readonly IQuizRepo _quizRepo;
        private readonly ILogger<QuizService> _logger;

        // Add static readonly field for options
        private static readonly JsonSerializerOptions _jsonOptions = new()
        {
            PropertyNameCaseInsensitive = true
        };

        public QuizService(ISongRepo songRepo, IChatGptService chatGptService, IQuizRepo quizRepo, ILogger<QuizService> logger)
        {
            _songRepo = songRepo;
            _chatGptService = chatGptService;
            _quizRepo = quizRepo;
            _logger = logger;
        }

        private bool HasDuplicateOptions(QuizResponseDto quiz)
        {
            foreach (QuizQuestionDto question in quiz.Questions)
            {
                // Create a HashSet to check for duplicates efficiently
                HashSet<string> uniqueOptions = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
                foreach (string option in question.Options)
                {
                    if (!uniqueOptions.Add(option))
                    {
                        _logger.LogWarning("Found duplicate option in question {QuestionNumber}: {Option}", 
                            question.QuestionNumber, option);
                        return true;
                    }
                }
            }
            return false;
        }

        public async Task<QuizResponseDto> GenerateQuizAsync(Guid songId, bool useCachedQuiz, int difficulty, int numQuestions)
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
                    try
                    {
                        // Update the deserialization to use cached options
                        QuizResponseDto? quizResponse = JsonSerializer.Deserialize<QuizResponseDto>(
                            cachedQuiz.QuizJson,
                            _jsonOptions);
                        if (quizResponse != null && !HasDuplicateOptions(quizResponse))
                        {
                            return quizResponse;
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Failed to deserialize cached quiz for songId {SongId}", songId);
                    }
                }
            }

            // Retrieve the song by songId and verify its lyrics
            Models.SongData.Song? song = await _songRepo.GetSongByIdAsync(songId) ?? throw new InvalidOperationException($"Song with ID {songId} not found in database.");
            if (string.IsNullOrWhiteSpace(song.LrcLyrics))
            {
                throw new InvalidOperationException("Song lyrics not available.");
            }

            // Maximum number of retries for quiz generation
            const int maxRetries = 3;
            QuizResponseDto quizResponseDto;
            int retryCount = 0;

            do
            {
                if (retryCount > 0)
                {
                    _logger.LogWarning("Retrying quiz generation due to duplicate options. Attempt {RetryCount} of {MaxRetries}", 
                        retryCount + 1, maxRetries);
                }

                // Generate a quiz by calling the ChatGPT service with the song lyrics
                quizResponseDto = await _chatGptService.GenerateRomanizationQuizAsync(song.LrcLyrics, difficulty, numQuestions, songId);
                retryCount++;

                if (!HasDuplicateOptions(quizResponseDto))
                {
                    break;
                }

                if (retryCount >= maxRetries)
                {
                    throw new InvalidOperationException("Failed to generate quiz without duplicate options after maximum retries.");
                }
            } while (true);

            // Override LLM-generated values with our own
            DateTime timestamp = DateTime.UtcNow;
            Guid quizId = Guid.NewGuid();
            QuizResponseDto quizResponseWithOverrides = quizResponseDto with
            {
                Timestamp = timestamp,
                QuizId = quizId
            };

            // Cache the quiz in the database
            Quiz quizEntity = new Quiz
            {
                Id = quizId,  // Use the same Guid for the entity
                SongId = songId,
                Difficulty = difficulty,
                NumQuestions = numQuestions,
                QuizJson = JsonSerializer.Serialize(quizResponseWithOverrides),
                Timestamp = timestamp
            };
            await _quizRepo.AddAsync(quizEntity);

            return quizResponseWithOverrides;
        }
    }
}
