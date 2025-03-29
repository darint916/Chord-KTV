using System.Diagnostics;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using ChordKTV.Dtos;
using ChordKTV.Dtos.TranslationGptApi;
using ChordKTV.Dtos.Quiz;
using ChordKTV.Services.Api;
using ChordKTV.Dtos.OpenAI;
using ChordKTV.Models.Quiz;
using System.Runtime.CompilerServices;

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

    // Add static readonly field for options
    private static readonly JsonSerializerOptions _jsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

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
            throw new ArgumentException($"Error in {nameof(TranslateLyricsAsync)}: At least one of romanize or translate must be true.");
        }
        string prompt = $@"
You are a helpful assistant that translates LRC formatted lyrics into an English translation (if needed) and, if needed, a romanized version (using the English alphabet) while maintaing the same format.
Is romanization needed: {(romanize ? "Yes" : "No")}
Is translation needed: {(translate ? "Yes" : "No")}
Notice the only exception to this is if the original lyrics are already in English, in which case no translation or romanization is needed. This occurs if language code is UNK, correct to EN if it is english and null out romanization and translation.
If not needed, have the json response for the section be null or not there. Maintain the LRC text format timestamps exactly in the responses.
If the language cannot be determined, auto-detect it instead of returning 'UNK'. Correct the suggested ISO 639-1 language code (2 letters if not uknown) if needed: {languageCode}.
If multiple languages are detected, use the most common non-English language code. If unknown make it 'UNK'.

Input Lyrics:
{originalLyrics}

Remember if the original lyrics are already in English, no translation or romanization is needed and ignore the is needed clauses above.
The lyrics input contains timestamps LRC Format. Do not change any timestamps or the formatting.
Ensure that the output follows the expected JSON structure.
Output Format:
{{
    ""romanizedLyrics"": ""<romanized lyrics in LRC format, if applicable or null>"",
    ""translatedLyrics"": ""<translated English lyrics in LRC format, if applicable or null>"",
    ""languageCode"": ""<valid ISO 639-1 language code>""
}}
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

        var sw = new Stopwatch();
        sw.Start();
        OpenAIResponseDto? openAIResponse = await GptChatCompletionAsync(JsonSerializer.Serialize(requestBody));
        sw.Stop();
        _logger.LogInformation("⏱️ ChatGPT API call took: {ElapsedMilliseconds}ms", sw.ElapsedMilliseconds);
        if (openAIResponse == null || openAIResponse.Choices.Count == 0)
        {
            _logger.LogError("No choices were returned from the ChatGPT API for TranslateLyrics.");
            throw new InvalidOperationException("No choices were returned from the ChatGPT API TranslateLyrics.");
        }
        string? messageContent = openAIResponse.Choices[0].Message.Content;
        TranslatedSongLyrics? translatedSongLyrics = JsonSerializer.Deserialize<TranslatedSongLyrics>(messageContent, _jsonOptions);
        if (translatedSongLyrics == null)
        {
            _logger.LogError("Failed to deserialize the ChatGPT API response for TranslateLyrics. messageContent: {MessageContent}", messageContent);
            throw new InvalidOperationException($"Error in {nameof(TranslateLyricsAsync)}: Failed to deserialize the ChatGPT API response.");
        }

        //Try parsing language code from string
        if (Enum.TryParse(translatedSongLyrics.LanguageCode, out LanguageCode parsedLanguageCode))
        {
            if (parsedLanguageCode == LanguageCode.UNK)
            {
                _logger.LogError("Unkown language code returned from ChatGPT API: {LanguageCode} \n Lyrics: {OriginalLyrics}", translatedSongLyrics.LanguageCode, originalLyrics);
            }
            else
            {
                languageCode = parsedLanguageCode;
            }
        }
        else
        {
            _logger.LogError("Failed to parse language code from ChatGPT API response: {LanguageCode} \n Lyrics: {OriginalLyrics}", translatedSongLyrics.LanguageCode, originalLyrics);
        }
        if (languageCode != LanguageCode.EN)
        {
            if ((romanize && string.IsNullOrEmpty(translatedSongLyrics.RomanizedLyrics)) || (translate && string.IsNullOrEmpty(translatedSongLyrics.TranslatedLyrics)))
            {
                _logger.LogError("messageContent: {MessageContent}", messageContent);
                throw new InvalidOperationException($"Error in {nameof(TranslateLyricsAsync)}: The ChatGPT API response did not contain the expected translations.");
            }
        }

        return new TranslationResponseDto
        {
            OriginalLyrics = originalLyrics,
            LanguageCode = languageCode,
            RomanizedLyrics = languageCode == LanguageCode.EN ? originalLyrics : translatedSongLyrics.RomanizedLyrics,
            TranslatedLyrics = languageCode == LanguageCode.EN ? originalLyrics : translatedSongLyrics.TranslatedLyrics,
        };
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
            throw new InvalidOperationException($"Error in {nameof(BatchTranslateLyricsAsync)} One or more translations failed during batch processing: {ex.Message}", ex);
        }
    }

    public async Task<Quiz> GenerateRomanizationQuizAsync(string lyrics, int difficulty, int numQuestions, Guid songId)
    {
        string difficultyInstruction = difficulty switch
        {
            1 => "### The incorrect answers must be literally entirely different sentences than the correct answer, but taken from the same original song. The wrong answers should be the same length as the original song lyric phrase.",
            2 => "### The incorrect answers must be literally entirely different words than the correct answer, but still the same length and shape as the original song lyric phrase.",
            3 => "### The incorrect answers must contain very exaggerated, unique, and different words, but that look similar to the correct answer. The wrong answers should be OBVIOUSLY different and incorrect.",
            4 => "### The incorrect answers must be similar to the correct answer, but not exactly the same.",
            5 => "### The incorrect answers must be very close to the correct answer, almost exactly the same.",
            _ => throw new ArgumentOutOfRangeException(nameof(difficulty), "Difficulty must be between 1 and 5")
        };

        string prompt = $@"
            You are a helpful assistant that generates multiple choice ENGLISH ROMANIZATION quizzes from song lyrics for romanization practice.
            The full lyrics are provided below:
            {lyrics}

            Your task:
            - Identify and select {numQuestions} key phrases from the lyrics that are significant for romanization.
            - The lyricPhrase has to be entirely non-latin.
            - For each key phrase, create a multiple-choice question with exactly 4 answer options (only one is correct, the first option is the correct romanization).
            - IMPORTANT: The first option (index 0) MUST ALWAYS be the correct romanization.
            - The difficulty of the quiz is set to {difficulty} on a scale from 1 (easiest) to 5 (hardest).
            - {difficultyInstruction}
            - All options must be written using the LATIN alphabet and NOTHING ELSE.
            - No two options should be the same, no matter the difficulty.
            - Respond with a JSON object exactly in the following format:
            {{
                ""questions"": [
                    {{
                        ""questionNumber"": 1,
                        ""lyricPhrase"": ""<extracted song lyric phrase that is entirely non-latin>"",
                        ""options"": [""<correct romanization>"", ""wrong1 in latin alphabet"", ""wrong2 in latin alphabet"", ""wrong3 in latin alphabet""]
                    }},
                    ... up to {numQuestions} questions
                ]
            }}
            Ensure that the JSON is the only output and does not include any additional text or explanation.
            All options must be written using the LATIN alphabet and NOTHING ELSE.";

        string systemPrompt = "You are an assistant specialized in generating romanization quizzes from song lyrics.";

        var requestBody = new
        {
            model = Model,
            messages = new object[]
            {
                new { role = "system", content = systemPrompt },
                new { role = "user", content = prompt }
            },
            temperature = 1.0
        };

        try
        {

            OpenAIResponseDto? openAIResponse = await GptChatCompletionAsync(JsonSerializer.Serialize(requestBody));

            if (openAIResponse == null || openAIResponse.Choices.Count == 0)
            {
                _logger.LogError("No choices were returned from the ChatGPT API for quiz generation.");
                throw new InvalidOperationException("No choices were returned from the ChatGPT API.");
            }

            string? messageContent = openAIResponse.Choices[0].Message.Content;

            if (string.IsNullOrWhiteSpace(messageContent))
            {
                _logger.LogError("The ChatGPT API returned an empty response for quiz generation.");
                throw new InvalidOperationException("The ChatGPT API returned an empty response.");
            }

            QuizResponseDto? gptResponse = JsonSerializer.Deserialize<QuizResponseDto>(messageContent, _jsonOptions);
            if (gptResponse?.Questions == null)
            {
                _logger.LogError("Failed to deserialize quiz response. Raw response: {MessageContent}", messageContent);
                throw new InvalidOperationException("Invalid quiz response format from ChatGPT.");
            }

            var quiz = new Quiz
            {
                Id = Guid.NewGuid(),
                SongId = songId,
                Difficulty = difficulty,
                NumQuestions = gptResponse.Questions.Count,
                Timestamp = DateTime.UtcNow,
                Questions = []
            };

            foreach (QuizQuestionDto questionDto in gptResponse.Questions)
            {
                var question = new QuizQuestion
                {
                    Id = Guid.NewGuid(),
                    QuizId = quiz.Id,
                    Quiz = quiz,
                    QuestionNumber = questionDto.QuestionNumber,
                    LyricPhrase = questionDto.LyricPhrase,
                    Options = []
                };

                for (int i = 0; i < questionDto.Options.Count; i++)
                {
                    question.Options.Add(new QuizOption
                    {
                        Id = Guid.NewGuid(),
                        QuestionId = question.Id,
                        Question = question,
                        Text = questionDto.Options[i],
                        IsCorrect = i == 0,
                        OrderIndex = i
                    });
                }

                quiz.Questions.Add(question);
            }

            return quiz;
        }
        catch (HttpRequestException httpEx)
        {
            _logger.LogError(httpEx, "HTTP request error while calling the ChatGPT API for quiz generation.");
            throw new HttpRequestException("HTTP request error while calling the ChatGPT API for quiz generation.", httpEx);
        }
    }

    // for youtube video title and channel name that enters the system, cant be found by genius
    public async Task<CandidateSongInfoListResponse> GetCandidateSongInfosAsync(string videoTitle, string channelName)
    {
        string prompt = $@"
        Extract the song title and artist from the following YouTube video title and channel name.
        Input:
        Video Title: ""{videoTitle}""
        Channel Name: ""{channelName}""
        Task:
        - Remove unnecessary words and noise such as ""MV"", ""Official"", ""Lyrics"", and other non-essential details that typically come in song titles.
        - Identify potential combinations of the song title and artist. If multiple valid versions exist (e.g., native and English), list them as candidates.
        - Format the output as a JSON object with a ""candidates"" array, where each candidate is an object with ""title"" and ""artist"" fields.
        - Use your knowledge of songs to put the most well known title and artist as first candidate. If the channel name is not the artist, ignore it and have the most likely artist as some videos are made by fans or other channels.
        Output Format:
        {{
        ""candidates"": [
            {{ ""title"": ""Song Title 1"", ""artist"": ""Artist Name 1"" }},
            {{ ""title"": ""Song Title 2"", ""artist"": ""Artist Name 2"" }}
        ]
        }}

        Example:
        Input:
        Video Title: ""韋禮安 WeiBird《如果可以 Red Scarf》MV - 電影「月老」主題曲導演親剪音樂視角版""
        Channel Name: ""韋禮安 WeiBird""

        Expected Output:
        {{
        ""candidates"": [
            {{ ""title"": ""如果可以"", ""artist"": ""韋禮安"" }},
            {{ ""title"": ""Red Scarf"", ""artist"": ""WeiBird"" }}
        ]
        }}
        ";
        string systemPrompt = "You are an AI assistant specializing in extracting or querying clean and precise song titles and artist names from YouTube video titles and channel names.";

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

        OpenAIResponseDto? openAIResponse = await GptChatCompletionAsync(JsonSerializer.Serialize(requestBody));
        if (openAIResponse == null || openAIResponse.Choices.Count == 0)
        {
            _logger.LogError("No choices were returned from the ChatGPT API for CandidateSongInfo extraction.");
            throw new InvalidOperationException("No choices were returned from the ChatGPT API CandidateSongInfo extraction.");
        }

        string? messageContent = openAIResponse.Choices[0].Message.Content;
        CandidateSongInfoListResponse? songInfoListResponse = JsonSerializer.Deserialize<CandidateSongInfoListResponse>(messageContent, _jsonOptions);
        if (songInfoListResponse == null || songInfoListResponse.Candidates.Count == 0)
        {
            _logger.LogError($"{nameof(GetCandidateSongInfosAsync)}: No candidates were returned from the ChatGPT API for CandidateSongInfo extraction.");
            throw new InvalidOperationException($"{nameof(GetCandidateSongInfosAsync)}: No candidates were returned from the ChatGPT API. Response content: {openAIResponse}");
        }
        return songInfoListResponse;
    }

    private async Task<OpenAIResponseDto?> GptChatCompletionAsync(string jsonRequest, [CallerMemberName] string caller = "")
    {
        try
        {
            using HttpRequestMessage requestMessage = new HttpRequestMessage(HttpMethod.Post, ChatGptEndpoint);
            requestMessage.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _apiKey);
            requestMessage.Content = new StringContent(jsonRequest, Encoding.UTF8, "application/json");
            using HttpResponseMessage response = await _httpClient.SendAsync(requestMessage);
            if (!response.IsSuccessStatusCode)
            {
                string errorContent = await response.Content.ReadAsStringAsync();
                throw new HttpRequestException($"Error in {nameof(GptChatCompletionAsync)} called from {caller}, ChatGPT API call failed with status code {response.StatusCode}: {errorContent}");
            }
            OpenAIResponseDto? openAIResponse = JsonSerializer.Deserialize<OpenAIResponseDto>(await response.Content.ReadAsStringAsync(), _jsonOptions);
            return openAIResponse;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error while calling the ChatGPT API.");
            throw new InvalidOperationException($"Caller: {caller}: Error while calling the ChatGPT API. {ex.Message}", ex);
        }
    }
}
