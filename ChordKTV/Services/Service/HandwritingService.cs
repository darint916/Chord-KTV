using System.Text.RegularExpressions;
using System.Globalization;
using Google.Cloud.Vision.V1;
using ChordKTV.Dtos;
using ChordKTV.Services.Api;

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
        if (!string.IsNullOrWhiteSpace(image.Language.ToString()) && Enum.IsDefined(typeof(LanguageCode), image.Language.ToString()))
        {
            imageContext.LanguageHints.Add(image.Language.ToString());
        }
        else
        {
            _logger.LogWarning("Invalid/No language specified. Defaulting to no language hint.");
        }

        _logger.LogDebug("Detecting text from image with language {Language}", image.Language.ToString());
        IReadOnlyList<EntityAnnotation> response = await client.DetectTextAsync(decodedImage, imageContext);

        string recognizedText = response.Count > 0 ? response[0].Description?.Trim() ?? string.Empty : string.Empty;

        _logger.LogDebug("Recognized text: {Text}", recognizedText);

        // Normalize texts: Remove spaces, newlines, and escape sequences
        string normalizedRecognized = NormalizeText(recognizedText);
        string normalizedExpected = NormalizeText(image.ExpectedMatch);

        // Compute similarity percentage
        double matchPercentage = CalculateSimilarityPercentage(normalizedRecognized, normalizedExpected);
        _logger.LogDebug("Match percentage: {MatchPercentage}%", matchPercentage);

        return new HandwritingCanvasResponseDto(recognizedText, matchPercentage);
    }

    [GeneratedRegex(@"\s+")] // Compile-time regex generation
    private static partial Regex WhitespaceRegex();

    private static string NormalizeText(string input)
    {
        return WhitespaceRegex().Replace(input, "").ToLower(CultureInfo.InvariantCulture); // Use InvariantCulture for consistency
    }

    private static int LevenshteinDistance(string source, string target)
    {
        int sourceLength = source.Length;
        int targetLength = target.Length;
        int[,] dp = new int[sourceLength + 1, targetLength + 1];

        for (int i = 0; i <= sourceLength; i++)
        {
            dp[i, 0] = i;
        }
        for (int j = 0; j <= targetLength; j++)
        {
            dp[0, j] = j;
        }

        for (int i = 1; i <= sourceLength; i++)
        {
            for (int j = 1; j <= targetLength; j++)
            {
                int cost = (source[i - 1] == target[j - 1]) ? 0 : 1;
                dp[i, j] = Math.Min(
                    Math.Min(dp[i - 1, j] + 1, dp[i, j - 1] + 1),
                    dp[i - 1, j - 1] + cost);
            }
        }
        return dp[sourceLength, targetLength];
    }

    private static double CalculateSimilarityPercentage(string recognizedText, string expectedText)
    {
        int maxLength = Math.Max(recognizedText.Length, expectedText.Length);
        if (maxLength == 0)
        {
            return 100.0;
        }

        int distance = LevenshteinDistance(recognizedText, expectedText);
        return Math.Max(0, 100.0 * (1.0 - ((double)distance / maxLength)));
    }
}
