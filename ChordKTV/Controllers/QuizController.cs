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
            [FromQuery] int geniusId,
            [FromQuery] bool useCachedQuiz = false,
            [FromQuery] int difficulty = 5,
            [FromQuery] int numQuestions = 5)
        {
            try
            {
                var quiz = await _quizService.GenerateQuizAsync(geniusId, useCachedQuiz, difficulty, numQuestions);
                return Ok(quiz);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating quiz for geniusId {GeniusId}", geniusId);
                return StatusCode(500, new { message = ex.Message });
            }
        }
    }
} 