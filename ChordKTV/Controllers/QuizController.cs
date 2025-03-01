using Microsoft.AspNetCore.Mvc;
using ChordKTV.Services.Api;
using ChordKTV.Dtos.Quiz;
using ChordKTV.Models.Quiz;

namespace ChordKTV.Controllers
{
    [ApiController]
    [Route("api/quiz")]
    public class QuizController : ControllerBase
    {
        private readonly IQuizService _quizService;
        private readonly ILogger<QuizController> _logger;

        public QuizController(IQuizService quizService, ILogger<QuizController> logger)
        {
            _quizService = quizService;
            _logger = logger;
        }

        [HttpGet("romanization")]
        public async Task<IActionResult> GetRomanizationQuiz(
            [FromQuery] Guid songId,
            [FromQuery] bool useCachedQuiz = false,
            [FromQuery] int difficulty = 3,
            [FromQuery] int numQuestions = 5)
        {
            try
            {
                if (difficulty is < 1 or > 5)
                {
                    return BadRequest(new { message = "Difficulty must be between 1 and 5" });
                }

                if (numQuestions < 1)
                {
                    return BadRequest(new { message = "Number of questions must be at least 1" });
                }
                if (numQuestions > 20)
                {
                    return BadRequest(new { message = "Number of questions cannot exceed 20" });
                }

                if (songId == Guid.Empty)
                {
                    return BadRequest(new { message = "Song ID is required" });
                }

                try
                {
                    Quiz quiz = await _quizService.GenerateQuizAsync(songId, useCachedQuiz, difficulty, numQuestions);
                    
                    // Map the entity to DTO for the API response
                    QuizResponseDto quizResponseDto = MapQuizToDto(quiz);
                    
                    return Ok(quizResponseDto);
                }
                catch (InvalidOperationException ex) when (ex.Message.Contains("not found"))
                {
                    return NotFound(new { message = ex.Message });
                }
                catch (InvalidOperationException ex) when (ex.Message.Contains("lyrics not available"))
                {
                    return BadRequest(new { message = ex.Message });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating quiz for songId {SongId}", songId);
                return StatusCode(500, new { message = "An unexpected error occurred while generating the quiz." });
            }
        }
        
        private static QuizResponseDto MapQuizToDto(Quiz quiz)
        {
            ArgumentNullException.ThrowIfNull(quiz);

            // Handle case where Questions is null
            if (quiz.Questions == null)
            {
                return new QuizResponseDto(
                    QuizId: quiz.Id,
                    SongId: quiz.SongId,
                    Difficulty: quiz.Difficulty,
                    Timestamp: quiz.Timestamp,
                    Questions: new List<QuizQuestionDto>()
                );
            }

            // Map quiz to DTO
            return new QuizResponseDto(
                QuizId: quiz.Id,
                SongId: quiz.SongId,
                Difficulty: quiz.Difficulty,
                Timestamp: quiz.Timestamp,
                Questions: quiz.Questions
                    .OrderBy(q => q.QuestionNumber)
                    .Select(q =>
                    {
                        List<QuizOption> orderedOptions = q.Options?
                            .OrderBy(o => o.OrderIndex)
                            .ToList() ?? new List<QuizOption>();
                            
                        return new QuizQuestionDto(
                            QuestionNumber: q.QuestionNumber,
                            LyricPhrase: q.LyricPhrase,
                            Options: orderedOptions.Select(o => o.Text).ToList(),
                            CorrectOptionIndex: orderedOptions.FindIndex(o => o.IsCorrect)
                        );
                    })
                    .ToList()
            );
        }
    }
}
