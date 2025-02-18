using Google.Cloud.Vision.V1;
using Microsoft.AspNetCore.Mvc;
using System.Text;
using ChordKTV.Dtos;

namespace ChordKTV.Controllers;

[Route("api/handwriting/ocr")]
[ApiController]
public class HandwritingController : Controller
{
    private readonly ILogger<HandwritingController> _logger;

    public HandwritingController(IConfiguration configuration, ILogger<HandwritingController> logger)
    {
        _logger = logger;
    }

    [HttpPost]
    [ProducesResponseType(typeof(string), 200)]
    [ProducesResponseType(typeof(string), 400)]
    [ProducesResponseType(typeof(string), 500)]
    public async Task<IActionResult> Recognize([FromBody] HandwritingCanvasDto image)
    {
        try
        {
            // Convert Base64 to Image
            byte[] imageBytes = Convert.FromBase64String(image.Image);
            var decodedImage = Image.FromBytes(imageBytes);

            // Initialize Google Cloud Vision Client
            var client = ImageAnnotatorClient.Create();

            // Set Language Hint
            var imageContext = new ImageContext();
            imageContext.LanguageHints.Add(image.Language.ToString()); // Specify language

            _logger.LogDebug("Detecting text from image with language {Language}", image.Language.ToString());
            IReadOnlyList<EntityAnnotation> response = await client.DetectTextAsync(decodedImage, imageContext);

            // Extract recognized text
            var recognizedText = new StringBuilder();
            foreach (EntityAnnotation? annotation in response)
            {
                recognizedText.AppendLine(annotation.Description);
            }

            return Ok(new { text = recognizedText.ToString().Trim() });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { error = ex.Message });
        }
    }
}
