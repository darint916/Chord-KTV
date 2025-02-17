using System.Diagnostics;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using ChordKTV.Dtos;
using ChordKTV.Dtos.TranslationGptApi;
using ChordKTV.Services.Api;

namespace ChordKTV.Services.Service;
public class ChatGptService : IChatGptService
{
    private readonly HttpClient _httpClient;
    private readonly string? _apiKey;
    private readonly ILogger<ChatGptService> _logger;
    private const string ChatGptEndpoint = "https://api.openai.com/v1/chat/completions";

    //Context window: 128,000 tokens, max tokens: 16,384 tokens , about 4 chars per token avg ~ 32000 chars, reference: Eminem love the way you lie : 4.4k chars
    //KR + other lang use more tokens, but as ref, https://platform.openai.com/tokenizer to calc, 2793 char -> 1564 tokens (sick enough to die)
    // price as of testing seems like ~$0.01 after 26k tokens lol
    private const string Model = "gpt-4o-mini"; //last updated 2024-07-18 , knowledge cutoff 10/2023

    public ChatGptService(HttpClient httpClient, IConfiguration configuration, ILogger<ChatGptService> logger)
    {
        _httpClient = httpClient;
        _apiKey = configuration["OpenAI:ApiKey"];// ?? throw new ArgumentNullException("OpenAI:ApiKey is missing in configuration");
        _logger = logger;
        if (string.IsNullOrEmpty(_apiKey))
        {
            throw new ArgumentNullException(nameof(configuration), "OpenAI:ApiKey is missing or not set in configuration.");
        }
    }

    public async Task<TranslationResponseDto> TranslateLyricsAsync(string originalLyrics, LanguageCode languageCode, bool romanize, bool translate)
    {
        // Construct the prompt. You can adjust this prompt as needed.
        if (!romanize && !translate)
        {
            throw new ArgumentException("At least one of romanize or translate must be true.");
        }
        string prompt = $@"
Input Lyrics:
{originalLyrics}

The lyrics input contains timestamps LRC Format. Do not change any timestamps or the formatting.
{(romanize && translate ? "Romanize the lyrics, english alphabet (if not in english already), while preserving LRC format exactly, then translate them into English." :
        romanize ? "Romanize the lyrics, english alphabet, while preserving LRC format exactly, no translation needed" :
        "Translate the lyrics into English while preserving the LRC format. No romanization needed.")}
Respond using this format, keep '---' only:
{(romanize ? "Romanized Lyrics Here" : "")}
---
{(translate ? "Translated English Meaning Lyrics Here (preserve the LRC Format)" : "")}
";

        string systemPrompt = $@"
You are a helpful assistant that translates LRC formatted lyrics into an English translation and, if needed, a romanized version (using the English alphabet) while maintaing the same format.";
        var requestBody = new
        {
            model = Model,
            messages = new object[]
            {
                    new { role = "system", content = systemPrompt },
                    new { role = "user", content = prompt }
            },
            temperature = 0.4,
            top_p = 0.9
        };

        string jsonRequest = JsonSerializer.Serialize(requestBody);
        using var requestMessage = new HttpRequestMessage(HttpMethod.Post, ChatGptEndpoint);
        requestMessage.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _apiKey);
        requestMessage.Content = new StringContent(jsonRequest, Encoding.UTF8, "application/json");

        try
        {
            // benchmark
            var sw = new Stopwatch();
            sw.Start();
            using HttpResponseMessage response = await _httpClient.SendAsync(requestMessage);
            sw.Stop();
            _logger.LogInformation("⏱️ ChatGPT API call took: {ElapsedMilliseconds}ms", sw.ElapsedMilliseconds);

            if (!response.IsSuccessStatusCode)
            {
                // You can add more detailed error logging here.
                string errorContent = await response.Content.ReadAsStringAsync();
                throw new HttpRequestException($"ChatGPT API call failed with status code {response.StatusCode}: {errorContent}");
            }

            string responseContent = await response.Content.ReadAsStringAsync();

            // Deserialize the response.
            using var document = JsonDocument.Parse(responseContent);
            JsonElement root = document.RootElement;

            // The ChatGPT API returns choices in an array.
            JsonElement choices = root.GetProperty("choices");
            if (choices.GetArrayLength() == 0)
            {
                _logger.LogError("No choices were returned from the ChatGPT API. responseContent: {ResponseContent}", responseContent);
                throw new InvalidOperationException("No choices were returned from the ChatGPT API.");
            }

            string? messageContent = choices[0]
                .GetProperty("message")
                .GetProperty("content")
                .GetString();

            if (string.IsNullOrWhiteSpace(messageContent))
            {
                _logger.LogError("The ChatGPT API returned an empty response. responseContent: {ResponseContent}", responseContent);
                throw new InvalidOperationException("The ChatGPT API returned an empty response.");
            }

            string[] sections = messageContent.Split("---");
            string? romanizedLyrics = sections[0].Trim();
            string? translatedLyrics = sections.Length > 1 ? sections[1].Trim() : null;
            romanizedLyrics = string.IsNullOrEmpty(romanizedLyrics) ? null : romanizedLyrics;
            translatedLyrics = string.IsNullOrEmpty(translatedLyrics) ? null : translatedLyrics;

            if ((romanize && romanizedLyrics == null) || (translate && translatedLyrics == null))
            {
                _logger.LogError("messageContent: {MessageContent}", messageContent);
                throw new InvalidOperationException("The ChatGPT API response did not contain the expected translations.");
            }
            return new TranslationResponseDto
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
            _logger.LogError(httpEx, "HTTP request error while calling the ChatGPT API.");
            throw new HttpRequestException("HTTP request error while calling the ChatGPT API.", httpEx);
        }
    }

    /// <inheritdoc/>
    public async Task<List<TranslationResponseDto>> BatchTranslateLyricsAsync(List<TranslationRequestDto> lrcLyrics)
    {
        //TODO: Revise for batch call to save on api calls
        // For batch processing, we can run multiple translation calls in parallel.
        IEnumerable<Task<TranslationResponseDto>> translationTasks = lrcLyrics.Select(request => TranslateLyricsAsync(request.OriginalLyrics, request.LanguageCode, request.Romanize, request.Translate));

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
