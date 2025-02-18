using Google.Cloud.Vision.V1;
using Microsoft.AspNetCore.Mvc;
using System.Text;

namespace ChordKTV.Controllers;

[Route("api/handwriting/ocr")]
[ApiController]
public class HandwritingController : Controller
{
    [HttpPost]
    [ProducesResponseType(typeof(string), 200)]
    [ProducesResponseType(typeof(string), 400)]
    [ProducesResponseType(typeof(string), 500)]
    public async Task<IActionResult> Recognize([FromBody] string image)
    {
        if (string.IsNullOrEmpty(image))
        {
            return BadRequest("No image data received");
        }

        try
        {
            // Convert Base64 to Image
            byte[] imageBytes = Convert.FromBase64String(image);
            var decodedImage = Image.FromBytes(imageBytes);

            // Initialize Google Cloud Vision Client
            var client = ImageAnnotatorClient.Create();


            // Set Japanese Language Hint
            var imageContext = new ImageContext();
            imageContext.LanguageHints.Add("ja"); // Specify Japanese language

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
