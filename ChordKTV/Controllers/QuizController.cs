using Microsoft.AspNetCore.Mvc;
using ChordKTV.Services.Api;
using ChordKTV.Dtos.Quiz;

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

                QuizResponseDto quiz = await _quizService.GenerateQuizAsync(songId, useCachedQuiz, difficulty, numQuestions);
                return Ok(quiz);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating quiz for songId {SongId}", songId);
                return StatusCode(500, new { message = ex.Message });
            }
        }
    }
}
