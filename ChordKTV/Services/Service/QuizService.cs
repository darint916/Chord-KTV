using System;
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

        private static Quiz MapDtoToEntity(QuizResponseDto dto)
        {
            Quiz quiz = new Quiz
            {
                Id = Guid.NewGuid(),
                SongId = dto.SongId,
                Difficulty = dto.Difficulty,
                NumQuestions = dto.Questions.Count,
                Timestamp = dto.Timestamp,
                Questions = new List<QuizQuestion>()
            };

            // Create questions and add them to the quiz
            foreach (QuizQuestionDto questionDto in dto.Questions)
            {
                QuizQuestion question = new QuizQuestion
                {
                    Id = Guid.NewGuid(),
                    QuizId = quiz.Id,
                    Quiz = quiz,  // Set navigation property
                    QuestionNumber = questionDto.QuestionNumber,
                    LyricPhrase = questionDto.LyricPhrase,
                    Options = new List<QuizOption>()
                };

                // Create options and add them to the question
                for (int i = 0; i < questionDto.Options.Count; i++)
                {
                    QuizOption option = new QuizOption
                    {
                        Id = Guid.NewGuid(),
                        QuestionId = question.Id,
                        Question = question,  // Set navigation property
                        Text = questionDto.Options[i],
                        OrderIndex = i,
                        IsCorrect = i == questionDto.CorrectOptionIndex
                    };
                    question.Options.Add(option);
                }

                quiz.Questions.Add(question);
            }

            return quiz;
        }

        private static QuizResponseDto MapEntityToDto(Quiz quiz)
        {
            ArgumentNullException.ThrowIfNull(quiz);

            // Handle case where Questions is null
            if (quiz.Questions == null)
            {
                return new QuizResponseDto(
                    quizId: quiz.Id,
                    songId: quiz.SongId,
                    difficulty: quiz.Difficulty,
                    timestamp: quiz.Timestamp,
                    questions: new List<QuizQuestionDto>()
                );
            }

            // Map quiz to DTO
            return new QuizResponseDto(
                quizId: quiz.Id,
                songId: quiz.SongId,
                difficulty: quiz.Difficulty,
                timestamp: quiz.Timestamp,
                questions: quiz.Questions
                    .OrderBy(q => q.QuestionNumber)
                    .Select(q =>
                    {
                        List<QuizOption> orderedOptions = q.Options?
                            .OrderBy(o => o.OrderIndex)
                            .ToList() ?? new List<QuizOption>();

                        return new QuizQuestionDto(
                            questionNumber: q.QuestionNumber,
                            lyricPhrase: q.LyricPhrase,
                            options: orderedOptions.Select(o => o.Text).ToList(),
                            correctOptionIndex: orderedOptions.FindIndex(o => o.IsCorrect)
                        );
                    })
                    .ToList()
            );
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

                    QuizResponseDto quizResponse = MapEntityToDto(cachedQuiz);

                    if (!HasDuplicateOptions(quizResponse))
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

                // Always update the timestamp to the current time instead of using the one from ChatGPT
                DateTime currentUtcTime = DateTime.UtcNow;
                quizResponseDto.Timestamp = currentUtcTime;

                _logger.LogDebug("Generated new quiz with ID: {QuizId}, updated timestamp to: {Timestamp}",
                    quizResponseDto.QuizId, quizResponseDto.Timestamp);

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

            // Convert DTO to entity and save
            Quiz quizEntity = MapDtoToEntity(quizResponseDto);

            // Double-check to ensure timestamp is current
            if (quizEntity.Timestamp.Date != DateTime.UtcNow.Date)
            {
                quizEntity.Timestamp = DateTime.UtcNow;
                _logger.LogDebug("Updated quiz timestamp to ensure current UTC time: {Timestamp}", quizEntity.Timestamp);
            }

            await _quizRepo.AddAsync(quizEntity);

            _logger.LogDebug("Successfully saved new quiz with ID: {QuizId}, Timestamp: {Timestamp}",
                quizEntity.Id, quizEntity.Timestamp);

            return quizEntity;
        }
    }
}
