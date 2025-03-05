using Microsoft.AspNetCore.Mvc;
using ChordKTV.Services.Api;
using ChordKTV.Dtos.Quiz;
using ChordKTV.Models.Quiz;
using AutoMapper;

namespace ChordKTV.Controllers
{
    [ApiController]
    [Route("api/quiz")]
    public class QuizController : ControllerBase
    {
        private readonly IQuizService _quizService;
        private readonly ILogger<QuizController> _logger;
        private readonly IMapper _mapper;

        public QuizController(IQuizService quizService, ILogger<QuizController> logger, IMapper mapper)
        {
            _quizService = quizService;
            _logger = logger;
            _mapper = mapper;
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

                    // Map the entity to DTO for the API response using AutoMapper
                    QuizResponseDto quizResponseDto = _mapper.Map<QuizResponseDto>(quiz);

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
    }
}
