using System;
using System.Threading.Tasks;
using ChordKTV.Data.Api.SongData;
using ChordKTV.Data.Api.QuizData;
using ChordKTV.Dtos.Quiz;
using ChordKTV.Models.Quiz;
using ChordKTV.Services.Api;
using Microsoft.Extensions.Logging;
using AutoMapper;

namespace ChordKTV.Services.Service;

public class QuizService : IQuizService
{
    private readonly ISongRepo _songRepo;
    private readonly IChatGptService _chatGptService;
    private readonly IQuizRepo _quizRepo;
    private readonly ILogger<QuizService> _logger;
    private readonly IMapper _mapper;

    public QuizService(ISongRepo songRepo, IChatGptService chatGptService, IQuizRepo quizRepo, ILogger<QuizService> logger, IMapper mapper)
    {
        _songRepo = songRepo;
        _chatGptService = chatGptService;
        _quizRepo = quizRepo;
        _logger = logger;
        _mapper = mapper;
    }

    private bool HasDuplicateOptions(QuizResponseDto quiz)
    {
        foreach (QuizQuestionDto question in quiz.Questions)
        {
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

                // Automap the cached entity to a DTO for checking duplicates
                QuizResponseDto quizResponse = _mapper.Map<QuizResponseDto>(cachedQuiz);

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

        // 3 is the maximum number of retries for quiz generation. 
        // It is already unlikely that the first attempt will have duplicate options. 
        // If we hit more than 3 retries, it means there is a problem with the input lyrics or the LLM. We should stop here to avoid an infinite loop.
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

            // Generate a quiz by calling ChatGPT
            quizResponseDto = await _chatGptService.GenerateRomanizationQuizAsync(song.LrcLyrics, difficulty, numQuestions, songId);

            // Overwrite the returned timestamp with the current time
            quizResponseDto.Timestamp = DateTime.UtcNow;

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

        // Map the quiz DTO to the entity using AutoMapper
        Quiz quizEntity = _mapper.Map<Quiz>(quizResponseDto);

        // We still generate a new ID to ensure we always treat it as a new quiz entry
        quizEntity.Id = Guid.NewGuid();

        await _quizRepo.AddAsync(quizEntity);

        _logger.LogDebug("Successfully saved new quiz with ID: {QuizId}, Timestamp: {Timestamp}",
            quizEntity.Id, quizEntity.Timestamp);

        return quizEntity;
    }
}
