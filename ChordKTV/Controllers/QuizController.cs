using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
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

        [AllowAnonymous]
        [HttpPost("romanization")]
        [ProducesResponseType(typeof(QuizResponseDto), 200)]
        [ProducesResponseType(404)]
        [ProducesResponseType(400)]
        [ProducesResponseType(500)]
        public async Task<IActionResult> GetRomanizationQuiz([FromBody] QuizRequestDto request)
        {
            return await ExecuteQuizRequest(
                request.SongId,
                request.UseCachedQuiz,
                request.Difficulty,
                request.NumQuestions,
                _quizService.GenerateQuizAsync,
                "romanization");
        }

        [AllowAnonymous]
        [HttpPost("audio")]
        [ProducesResponseType(typeof(QuizResponseDto), 200)]
        [ProducesResponseType(404)]
        [ProducesResponseType(400)]
        [ProducesResponseType(500)]
        public async Task<IActionResult> GetAudioQuiz([FromBody] QuizRequestDto request)
        {
            return await ExecuteQuizRequest(
                request.SongId,
                request.UseCachedQuiz,
                request.Difficulty,
                request.NumQuestions,
                _quizService.GenerateAudioQuizAsync,
                "audio");
        }

        /// A DRY helper that runs the shared validation / try–catch / mapping logic,
        /// but calls the quizGenerator delegate for the actual quiz creation.
        private async Task<IActionResult> ExecuteQuizRequest(
            Guid songId,
            bool useCachedQuiz,
            int difficulty,
            int numQuestions,
            Func<Guid, bool, int, int, Task<Quiz>> quizGenerator,
            string quizTypeForLogs)
        {
            // 1) validation
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
                // 2) actual quiz‐making
                Quiz quiz = await quizGenerator(songId, useCachedQuiz, difficulty, numQuestions);

                // 3) map to DTO & return
                QuizResponseDto dto = _mapper.Map<QuizResponseDto>(quiz);
                return Ok(dto);
            }
            catch (InvalidOperationException ex) when (ex.Message.Contains("not found"))
            {
                return NotFound(new { message = ex.Message });
            }
            catch (InvalidOperationException ex) when (ex.Message.Contains("lyrics not available"))
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Error generating {QuizType} quiz for songId {SongId}",
                    quizTypeForLogs,
                    songId);
                return StatusCode(500, new { message = $"An unexpected error occurred while generating the {quizTypeForLogs} quiz." });
            }
        }
    }
}
