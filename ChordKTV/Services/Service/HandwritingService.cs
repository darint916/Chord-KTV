using System.Text.RegularExpressions;
using System.Globalization;
using Google.Cloud.Vision.V1;
using ChordKTV.Services.Api;
using FuzzySharp;
using ChordKTV.Dtos.HandwritingCanvas;

namespace ChordKTV.Services.Service;

public partial class HandwritingService : IHandwritingService
{
    private readonly ILogger<HandwritingService> _logger;

    public HandwritingService(ILogger<HandwritingService> logger)
    {
        _logger = logger;
    }

    public async Task<HandwritingCanvasResponseDto?> RecognizeTextAsync(HandwritingCanvasRequestDto image)
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
        imageContext.LanguageHints.Add(image.Language.ToString());

        _logger.LogDebug("Detecting text from image with language {Language}", image.Language.ToString());
        IReadOnlyList<EntityAnnotation> response = await client.DetectTextAsync(decodedImage, imageContext);

        string recognizedText = response.Count > 0 ? response[0].Description?.Trim() ?? string.Empty : string.Empty;

        _logger.LogDebug("Recognized text: {Text}", recognizedText);

        // Normalize texts: Remove spaces, newlines, and escape sequences
        string normalizedRecognized = NormalizeText(recognizedText);
        string normalizedExpected = NormalizeText(image.ExpectedMatch);

        // Compute similarity percentage
        double matchPercentage = Fuzz.Ratio(normalizedRecognized, normalizedExpected);
        _logger.LogDebug("Match percentage: {MatchPercentage}%", matchPercentage);

        return new HandwritingCanvasResponseDto(recognizedText, matchPercentage);
    }

    [GeneratedRegex(@"\s+")] // Compile-time regex generation
    private static partial Regex WhitespaceRegex();

    private static string NormalizeText(string input)
    {
        return WhitespaceRegex().Replace(input, "").ToLower(CultureInfo.InvariantCulture); // Use InvariantCulture for consistency
    }
}
