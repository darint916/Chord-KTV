using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using ChordKTV.Dtos;
using ChordKTV.Services.Api;

namespace ChordKTV.Services.Service;
public class ChatGptService : IChatGptService
{
    private readonly HttpClient _httpClient;
    private readonly string? _apiKey;
    private const string ChatGptEndpoint = "https://api.openai.com/v1/chat/completions";

    //Context window: 128,000 tokens, max tokens: 16,384 tokens , about 4 chars per token avg ~ 32000 chars, reference: Eminem love the way you lie : 4.4k chars
    //KR + other lang use more tokens, but as ref, https://platform.openai.com/tokenizer to calc, 2793 char -> 1564 tokens (sick enough to die)
    private const string Model = "gpt-4o-mini"; //last updated 2024-07-18 , knowledge cutoff 10/2023

    public ChatGptService(HttpClient httpClient, IConfiguration configuration)
    {
        _httpClient = httpClient;
        _apiKey = configuration["OpenAI:ApiKey"];// ?? throw new ArgumentNullException("OpenAI:ApiKey is missing in configuration");
    }

    public async Task<LrcLyricsDto> TranslateLyricsAsync(string originalLyrics, LanguageCode languageCode)
    {
        // Construct the prompt. You can adjust this prompt as needed.
        string prompt = $@"
            You are a helpful assistant that romanizes and translates lyrics.
            The input is in LRC format with timestamps and a language subscript in ISO 639 (e.g. _{languageCode}).
            Translate the lyrics into their romanized version (i.e. using the Latin alphabet) while preserving the LRC format exactly and hen translate it to English
            Do not change any timestamps or the formatting.
            Respond using this format keeping the '---' delimiter between your response section:
            [Romanized Lyrics]
            ---
            [Translated English Lyrics]

            Input:
            {originalLyrics}";

        var requestBody = new
        {
            model = Model,
            messages = new object[]
            {
                    new { role = "system", content = "You are a helpful assistant that translates LRC formatted lyrics into english and romanized version." },
                    new { role = "user", content = prompt }
            },
            temperature = 0
        };

        var jsonRequest = JsonSerializer.Serialize(requestBody);
        using var requestMessage = new HttpRequestMessage(HttpMethod.Post, ChatGptEndpoint);
        requestMessage.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _apiKey);
        requestMessage.Content = new StringContent(jsonRequest, Encoding.UTF8, "application/json");

        try
        {
            using var response = await _httpClient.SendAsync(requestMessage);

            if (!response.IsSuccessStatusCode)
            {
                // You can add more detailed error logging here.
                var errorContent = await response.Content.ReadAsStringAsync();
                throw new HttpRequestException($"ChatGPT API call failed with status code {response.StatusCode}: {errorContent}");
            }

            var responseContent = await response.Content.ReadAsStringAsync();

            // Deserialize the response.
            using var document = JsonDocument.Parse(responseContent);
            var root = document.RootElement;

            // The ChatGPT API returns choices in an array.
            var choices = root.GetProperty("choices");
            if (choices.GetArrayLength() == 0)
            {
                throw new InvalidOperationException("No choices were returned from the ChatGPT API.");
            }

            var messageContent = choices[0]
                .GetProperty("message")
                .GetProperty("content")
                .GetString();

            if (string.IsNullOrWhiteSpace(messageContent))
            {
                throw new InvalidOperationException("The ChatGPT API returned an empty response.");
            }

            string[] sections = messageContent.Split("---", StringSplitOptions.RemoveEmptyEntries);
            if (sections.Length < 2)
            {
                throw new InvalidOperationException("The ChatGPT API response does not contain the expected sections.");
            }
            string romanizedLyrics = sections[0].Trim();
            string translatedLyrics = sections[1].Trim();

            return new LrcLyricsDto
            {
                OriginalLyrics = originalLyrics,
                LanguageCode = languageCode,
                RomanizedLyrics = romanizedLyrics,
                TranslatedLyrics = translatedLyrics
            };
        }
        catch (HttpRequestException httpEx)
        {
            // Log the error if logging is available.
            throw new HttpRequestException("HTTP request error while calling the ChatGPT API.", httpEx);
        }
        catch (Exception ex)
        {
            // Log or rethrow as needed.
            throw new InvalidOperationException("An error occurred while processing the ChatGPT API response.", ex);
        }
    }

    /// <inheritdoc/>
    public async Task<List<LrcLyricsDto>> BatchTranslateLyricsAsync(List<TranslationRequestDto> lrcLyrics)
    {
        //TODO: Revise for batch call to save on api calls
        // For batch processing, we can run multiple translation calls in parallel.
        var translationTasks = lrcLyrics.Select(request => TranslateLyricsAsync(request.OriginalLyrics, request.LanguageCode));

        try
        {
            return [.. await Task.WhenAll(translationTasks)];
        }
        catch (Exception ex)
        {
            // Handle or log batch errors appropriately.
            throw new InvalidOperationException("One or more translations failed during batch processing.", ex);
        }
    }
}
