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

        public QuizService(ISongRepo songRepo, IChatGptService chatGptService, IQuizRepo quizRepo, ILogger<QuizService> logger)
        {
            _songRepo = songRepo;
            _chatGptService = chatGptService;
            _quizRepo = quizRepo;
            _logger = logger;
        }

        public async Task<QuizResponseDto> GenerateQuizAsync(int geniusId, bool useCachedQuiz, int difficulty, int numQuestions)
        {
            // Clamp difficulty between 1 and 5
            difficulty = Math.Clamp(difficulty, 1, 5);
            
            // Clamp number of questions between 1 and 20
            numQuestions = Math.Clamp(numQuestions, 1, 20);

            // Try to use a cached quiz if requested
            if (useCachedQuiz)
            {
                var cachedQuiz = await _quizRepo.GetLatestQuizAsync(geniusId, difficulty);
                if (cachedQuiz != null)
                {
                    try
                    {
                        var quizResponse = JsonSerializer.Deserialize<QuizResponseDto>(cachedQuiz.QuizJson, 
                            new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                        if (quizResponse != null)
                        {
                            return quizResponse;
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Failed to deserialize cached quiz for geniusId {GeniusId}", geniusId);
                    }
                }
            }

            // Retrieve the song by geniusId and verify its lyrics
            var song = await _songRepo.GetSongByGeniusIdAsync(geniusId);
            if (song == null)
            {
                throw new InvalidOperationException($"Song with geniusID {geniusId} not found in database.");
            }
            if (string.IsNullOrWhiteSpace(song.LrcLyrics))
            {
                throw new InvalidOperationException("Song lyrics not available.");
            }

            // Generate a quiz by calling the ChatGPT service with the song lyrics
            var quizResponseDto = await _chatGptService.GenerateRomanizationQuizAsync(song.LrcLyrics, difficulty, numQuestions, geniusId);

            // Override LLM-generated values with our own
            var timestamp = DateTime.UtcNow;
            var quizId = Guid.NewGuid();
            var quizResponseWithOverrides = quizResponseDto with 
            { 
                Timestamp = timestamp,
                QuizId = quizId
            };

            // Cache the quiz in the database
            var quizEntity = new Quiz
            {
                Id = quizId,  // Use the same Guid for the entity
                GeniusId = geniusId,
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