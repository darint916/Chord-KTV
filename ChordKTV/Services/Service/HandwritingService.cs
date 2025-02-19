using Google.Cloud.Vision.V1;
using ChordKTV.Dtos;
using ChordKTV.Services.Api;

namespace ChordKTV.Services.Service;

public class HandwritingService : IHandwritingService
{
    private readonly ILogger<HandwritingService> _logger;

    public HandwritingService(ILogger<HandwritingService> logger)
    {
        _logger = logger;
    }

    public async Task<bool?> RecognizeTextAsync(HandwritingCanvasDto image)
    {
        if (string.IsNullOrWhiteSpace(image.Image))
        {
            _logger.LogError("Invalid image input: Image data is null or empty.");
            return null;
        }

        // Convert Base64 to Image
        byte[] imageBytes;
        try
        {
            imageBytes = Convert.FromBase64String(image.Image);
        }
        catch (FormatException ex)
        {
            _logger.LogError("Invalid Base64 format: {Message}", ex.Message);
            return null;
        }

        var decodedImage = Image.FromBytes(imageBytes);

        // Initialize Google Cloud Vision Client
        var client = ImageAnnotatorClient.Create();

        // Set Language Hint
        var imageContext = new ImageContext();
        if (!string.IsNullOrWhiteSpace(image.Language.ToString()))
        {
            imageContext.LanguageHints.Add(image.Language.ToString());
        }
        else
        {
            _logger.LogWarning("No language specified. Defaulting to no language hint.");
        }

        _logger.LogDebug("Detecting text from image with language {Language}", image.Language.ToString());
        IReadOnlyList<EntityAnnotation> response = await client.DetectTextAsync(decodedImage, imageContext);

        string recognizedText = response.Count > 0 ? response[0].Description?.Trim() ?? string.Empty : string.Empty;

        _logger.LogDebug("Recognized text: {Text}", recognizedText);

        // Compare with expected text (case-insensitive)
        return string.Equals(recognizedText, image.ExpectedMatch, StringComparison.OrdinalIgnoreCase);
    }
}
