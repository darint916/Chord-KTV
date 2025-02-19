using Microsoft.AspNetCore.Mvc;
using ChordKTV.Dtos;
using ChordKTV.Services.Api;

namespace ChordKTV.Controllers;

[Route("api/handwriting/ocr")]
[ApiController]
public class HandwritingController : Controller
{
    private readonly ILogger<HandwritingController> _logger;
    private readonly IHandwritingService _handwritingService;

    public HandwritingController(IHandwritingService handwritingService, ILogger<HandwritingController> logger)
    {
        _logger = logger;
        _handwritingService = handwritingService;

    }

    [HttpPost]
    [ProducesResponseType(typeof(HandwritingCanvasResponseDto), 200)]
    [ProducesResponseType(typeof(string), 400)]
    [ProducesResponseType(typeof(string), 500)]
    public async Task<IActionResult> Recognize([FromBody] HandwritingCanvasRequestDto image)
    {
        try
        {
            HandwritingCanvasResponseDto? match = await _handwritingService.RecognizeTextAsync(image);
            if (match == null)
            {
                return StatusCode(400, new { error = "Image input is null/invalid base64." });
            }
            return Ok(match);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to recognize handwriting.");
            return StatusCode(500, new { error = "An error occurred while processing the image." });
        }
    }
}
