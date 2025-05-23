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
using System.Text.RegularExpressions;

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
        object? requestBodyRom = null, requestBodyTrans = null;
        if (romanize)
        {
            string sysPrompt = $@"You are a JSON‐output specialist.  Given LRC‐format lyrics, emit **exactly one** JSON object formatted as such and nothing else:
**Japanese** ⇒ Always use **Revised Hepburn**, preserving every conjugated ending and vowel length (hiragana is one to one)  e.g. 触れればすべて思い出して → **furereba subete omoidashite**
**Chinese** ⇒ Hanyu Pinyin
**Cyrillic** ⇒ ISO‐9
**Arabic** ⇒ ALA‐LC
**etc.** for any other script
            {{
                ""romanizedLyrics"": ""< single string in LRC format where
– every bracketed timestamp is preserved exactly,
– any Latin letters, digits, punctuation, and spacing are copied verbatim,
- Whenever you see text in a non-Latin script, apply that language’s most widely accepted official romanization scheme, preserving all original morphological endings and diacritics (e.g. Pinyin for Chinese, Revised Hepburn for Japanese, ISO 9 for Cyrillic, ALA-LC for Arabic, etc.).>"",
                ""languageCode"": ""<the ISO 639-1 code of the primary non-Latin language detected (or ""en"" if all text is Latin).  >""
            }}";
            string userPrompt = $@"
                Here are the lyrics in LRC format.  Please produce the JSON described in the system prompt:
                {originalLyrics}";
            requestBodyRom = new
            {
                model = Model,
                messages = new object[]
                {
                    new { role = "system", content = sysPrompt },
                    new { role = "user", content = userPrompt }
                },
                temperature = 0,
                top_p = 1
            };
        }
        if (translate)
        {
            string sysPrompt = $@"You are a JSON‐output specialist.  Given LRC‐format lyrics, emit **exactly one** JSON object formatted as such and nothing else:
            {{
                ""translatedLyrics"": ""<a single string in LRC format where:
– each bracketed timestamp is preserved exactly,
– any English (Latin) text is copied verbatim,
– any non-English text is translated into natural, idiomatic English,
– blank lines remain blank lines with their original timestamps (if any).>>""
Do NOT include any additional fields, comments, or markdown—only the JSON object.
                ""languageCode"": ""<the ISO 639-1 code of the primary non-English language in the lyrics (use ""EN"" if all lines are already English, or ""UNK"" if you truly cannot detect it).>";
            string prompt = $@"
                Here are the lyrics in LRC format.  Please produce the JSON described in the system prompt:
                {originalLyrics}";
            requestBodyTrans = new
            {
                model = Model,
                messages = new object[]
                {
                        new { role = "system", content = sysPrompt },
                        new { role = "user", content = prompt }
                },
                temperature = 0.2,
                top_p = 1
            };
        }
        Task<OpenAIResponseDto?> romTask = romanize ? GptChatCompletionAsync(JsonSerializer.Serialize(requestBodyRom)) : Task.FromResult<OpenAIResponseDto?>(null);
        Task<OpenAIResponseDto?> transTask = translate ? GptChatCompletionAsync(JsonSerializer.Serialize(requestBodyTrans)) : Task.FromResult<OpenAIResponseDto?>(null);
        var sw = new Stopwatch();
        sw.Start();
        await Task.WhenAll(romTask, transTask);
        sw.Stop();
        _logger.LogInformation("⏱️ ChatGPT API call took: {ElapsedMilliseconds}ms", sw.ElapsedMilliseconds);

        string? romMsgContent = romTask.Result?.Choices[0].Message.Content;
        string? transMsgContent = transTask.Result?.Choices[0].Message.Content;
        if (romanize && string.IsNullOrWhiteSpace(romMsgContent))
        {
            _logger.LogError("Null Content was returned from the ChatGPT API for Romanization in TranslateLyrics.");
            throw new InvalidOperationException("Null Content was returned from the Romanization in ChatGPT API TranslateLyrics.");
        }
        if (translate && string.IsNullOrWhiteSpace(transMsgContent))
        {
            _logger.LogError("Null Content was returned from the ChatGPT API for Translation in TranslateLyrics.");
            throw new InvalidOperationException("Null Content was returned from the Translation in ChatGPT API TranslateLyrics.");
        }

        TranslatedSongLyrics? translationResponseDto = null;
        TranslatedSongLyrics? romanizedResponseDto = null;
        try //Deserializer may fail (if format wrong)
        {
            // Process romanizedLyrics field
            if (!string.IsNullOrEmpty(romMsgContent))
            {
                string pattern1 = @"(""romanizedLyrics""\s*:\s*"")(.+?)(""\s*,\s*""languageCode"")";
                romMsgContent = Regex.Replace(romMsgContent, pattern1, match =>
                {
                    string prefix = match.Groups[1].Value;
                    string content = match.Groups[2].Value;
                    string suffix = match.Groups[3].Value;

                    // Escape any unescaped quotes in the content
                    string escapedContent = Regex.Replace(content, @"(?<!\\)""", @"\""");

                    return prefix + escapedContent + suffix;
                }, RegexOptions.Singleline);

                romanizedResponseDto = JsonSerializer.Deserialize<TranslatedSongLyrics?>(romMsgContent, _jsonOptions);

                if (romanizedResponseDto is null)
                {
                    _logger.LogError("Failed to deserialize the RomanizedSongLyrics object. Raw response: {MessageContent}", romMsgContent);
                    throw new InvalidOperationException($"Error in {nameof(TranslateLyricsAsync)}: Failed to deserialize the RomanizedSongLyrics object.");
                }
                if (Enum.TryParse(romanizedResponseDto.LanguageCode.ToUpperInvariant(), out LanguageCode langCode))
                {
                    if (langCode == LanguageCode.UNK)
                    {
                        _logger.LogError("Unknown language code returned from ChatGPT API: {LanguageCode} \n Lyrics: {OriginalLyrics}", romanizedResponseDto.LanguageCode, originalLyrics);
                    }
                    else
                    {
                        languageCode = langCode;
                    }
                }
            }
            // Process translatedLyrics field
            if (!string.IsNullOrEmpty(transMsgContent))
            {
                string pattern2 = @"(""translatedLyrics""\s*:\s*"")(.+?)(""\s*,\s*""languageCode"")";
                transMsgContent = Regex.Replace(transMsgContent, pattern2, match =>
                {
                    string prefix = match.Groups[1].Value;
                    string content = match.Groups[2].Value;
                    string suffix = match.Groups[3].Value;

                    // Escape any unescaped quotes in the content
                    string escapedContent = Regex.Replace(content, @"(?<!\\)""", @"\""");

                    return prefix + escapedContent + suffix;
                }, RegexOptions.Singleline);

                translationResponseDto = JsonSerializer.Deserialize<TranslatedSongLyrics>(transMsgContent, _jsonOptions);

                if (translationResponseDto is null)
                {
                    _logger.LogError("Failed to deserialize the TranslatedSongLyrics object. Raw response: {MessageContent}", transMsgContent);
                    throw new InvalidOperationException($"Error in {nameof(TranslateLyricsAsync)}: Failed to deserialize the TranslatedSongLyrics object.");
                }
                if (Enum.TryParse(translationResponseDto.LanguageCode.ToUpperInvariant(), out LanguageCode langCode))
                {
                    if (langCode == LanguageCode.UNK)
                    {
                        _logger.LogError("Unknown language code returned from ChatGPT API: {LanguageCode} \n Lyrics: {OriginalLyrics}", translationResponseDto.LanguageCode, originalLyrics);
                    }
                    else
                    {
                        if (romanizedResponseDto != null && !string.Equals(romanizedResponseDto.LanguageCode, translationResponseDto.LanguageCode, StringComparison.OrdinalIgnoreCase))
                        {
                            _logger.LogError("Language code mismatch between romanized and translated lyrics: {RomanizedLanguageCode} vs {TranslatedLanguageCode} \n Lyrics: {OriginalLyrics}", romanizedResponseDto.LanguageCode, translationResponseDto.LanguageCode, originalLyrics);
                        }
                        languageCode = langCode; //we will take the second one (seems more reliable from translation prompt)
                    }
                }
            }
        }
        catch (JsonException ex)
        {
            _logger.LogError(ex, "Failed to deserialize the ChatGPT API response for TranslateLyrics. messageContent: {MessageContent}", romMsgContent ?? transMsgContent);
            throw new InvalidOperationException($"Error in {nameof(TranslateLyricsAsync)}: Failed to deserialize the ChatGPT API response.");
        }

        if (languageCode != LanguageCode.EN)
        {
            if ((romanize && string.IsNullOrEmpty(romanizedResponseDto?.RomanizedLyrics)) || (translate && string.IsNullOrEmpty(translationResponseDto?.TranslatedLyrics)))
            {
                _logger.LogError("messageContent: {MessageContent}", romMsgContent ?? transMsgContent);
                throw new InvalidOperationException($"Error in {nameof(TranslateLyricsAsync)}: The ChatGPT API response did not contain the expected translations.");
            }
        }

        return new TranslationResponseDto
        {
            OriginalLyrics = originalLyrics,
            LanguageCode = languageCode,
            RomanizedLyrics = languageCode == LanguageCode.EN ? originalLyrics : romanizedResponseDto?.RomanizedLyrics,
            TranslatedLyrics = languageCode == LanguageCode.EN ? originalLyrics : translationResponseDto?.TranslatedLyrics,
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
        Extract the song title and artist from the following YouTube video or Genius Source title and channel name.
        Input:
        Video Title: ""{videoTitle}""
        Channel Name: ""{channelName}""
        Task:
        - Remove unnecessary words and noise such as ""MV"", ""Official"", ""Lyrics"", and other non-essential details that typically come in song titles.
        - Identify potential combinations of the song title and artist. If multiple valid versions exist (e.g., native and English), list them as candidates.
        - Format the output as a JSON object with a ""candidates"" array, where each candidate is an object with ""title"" and ""artist"" fields.
        - Use your knowledge of songs to put the most well known title and artist as first candidate. If the channel name is not the artist, ignore it and have the most likely artist as some videos are made by fans or other channels.
        - Have an additional last entry for keeping the channel name as the artist but with an unnoisy title.
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
            {{ ""title"": ""如果可以"" , ""artist"": ""韋禮安 WeiBird"" }}
        ]
        }}

        Example:
        Input:
        Title: ""テトリス / 重音テトSV""
        Channel Name: ""柊マグネタイト""
        Expected Output:
        {{
        ""candidates"": [
            {{ ""title"": ""テトリス"", ""artist"": ""重音テトSV"" }},
            {{ ""title"": ""Tetris"", ""artist"": ""Kasane Teto"" }},
            {{ ""title"": ""テトリス"", ""artist"": ""柊マグネタイト"" }}
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

    public async Task<List<string>> GenerateAudioQuizDistractorsAsync(string correctLyric, int difficulty)
    {
        if (string.IsNullOrWhiteSpace(correctLyric))
        {
            throw new ArgumentException("Lyric must not be empty", nameof(correctLyric));
        }

        if (difficulty is < 1 or > 5)
        {
            throw new ArgumentOutOfRangeException(nameof(difficulty), "Difficulty must be between 1 and 5");
        }

        // 1) Build prompts
        string systemPrompt =
            "You are an assistant that generates plausible yet incorrect lyric lines " +
            "as distractors for an audio‐based quiz. Do not repeat the correct line.";

        string userPrompt = $@"
The correct lyric line is:
""{correctLyric}""

Difficulty level: {difficulty} (1 = easiest, 5 = hardest).

Your task:
- Generate exactly 3 incorrect options (distractors).
- Each must be plausible in the same language/style, about the same length.
- Do NOT repeat or overlap with the correct lyric.
- Return ONLY a JSON array of 3 strings, e.g. [""distractor1"",""distractor2"",""distractor3""] with no extra commentary.
";

        var requestBody = new
        {
            model = Model,
            messages = new object[]
            {
                new { role = "system", content = systemPrompt },
                new { role = "user",   content = userPrompt }
            },
            temperature = 0.7,
            top_p = 0.9
        };

        // 2) Call ChatGPT
        OpenAIResponseDto? openAIResponse =
            await GptChatCompletionAsync(JsonSerializer.Serialize(requestBody), nameof(GenerateAudioQuizDistractorsAsync));

        if (openAIResponse == null || openAIResponse.Choices.Count == 0)
        {
            throw new InvalidOperationException("No response choices from ChatGPT for audio distractors.");
        }

        string raw = openAIResponse.Choices[0].Message.Content?.Trim()
                   ?? throw new InvalidOperationException("Empty content in ChatGPT response.");

        // 3) Parse JSON array
        List<string>? distractors;
        try
        {
            distractors = JsonSerializer.Deserialize<List<string>>(raw, _jsonOptions);
        }
        catch (JsonException je)
        {
            _logger.LogError(je, "Failed to parse distractors JSON: {Raw}", raw);
            throw new InvalidOperationException("Invalid JSON format for audio distractors.");
        }

        if (distractors == null || distractors.Count != 3)
        {
            throw new InvalidOperationException($"Expected exactly 3 distractors, but got {distractors?.Count ?? 0}");
        }

        return distractors;
    }

    public async Task<TranslatePhrasesResponseDto> TranslateRomanizeAsync(string[] phrases, LanguageCode languageCode, int difficulty = 3)
    {
        if (phrases == null || phrases.Length == 0)
        {
            throw new ArgumentException("Phrases must not be empty", nameof(phrases));
        }

        string sysPrompt = $@"
You are a JSON‐output specialist. You will be given:
- A JSON array of user‐supplied phrases (order does not matter).
- A complexity level from 1 to 10.
Your task:
1. Ignore all symbols and punctuation in the inputs.
2. Based on the complexity level:
   • Determine **N = min(10, complexity + 2)** total items.
   • If complexity ≤ 3: choose only **single words**.
   • If complexity 4–7: choose **short phrases** of up to **3 words**.
   • If complexity 8–10: you may use phrases up to around **5 words**, but **no more** preferably.
3. Extract the top N **complete** words/phrases that make the best vocabulary practice for that complexity and difficulty to write.
3. For each extracted item, produce:
   • ""original"": the word or phrase exactly as from the input (minus symbols)
   • ""romanized"": the correct romanization (Revised Hepburn for Japanese, Pinyin for Chinese, ISO-9 for Cyrillic, ALA-LC for Arabic, etc.)
   • ""translated"": a natural, idiomatic English translation
4. Emit **exactly one** JSON object and nothing else, matching this schema:
emit **exactly one** JSON object and nothing else. The output JSON must match this schema:
{{
  ""phrases"": [
    {{
      ""original"": ""<complete word or short phrase>"",
      ""romanized"": ""<romanization>"",
      ""translated"": ""<English translation>""
    }},
    …
  ]
}}

Ensure there are no extra fields, comments, or markup in the output.";

        string userPrompt = $@"
LanguageHint: {languageCode}
Phrases:
{JsonSerializer.Serialize(phrases)}
Complexity: {difficulty}";


        var requestBody = new
        {
            model = Model,
            messages = new object[]
            {
                new { role = "system", content = sysPrompt.Trim() },
                new { role = "user",   content = userPrompt.Trim() }
            },
            temperature = 0.0,
            top_p = 1.0
        };
        OpenAIResponseDto? openAIResponse = await GptChatCompletionAsync(JsonSerializer.Serialize(requestBody));
        if (openAIResponse == null || openAIResponse.Choices.Count == 0)
        {
            throw new InvalidOperationException("No response choices from ChatGPT for TranslateRomanizeAsync.");
        }
        string messageContent = openAIResponse.Choices[0].Message.Content?.Trim()
                   ?? throw new InvalidOperationException("Empty content in ChatGPT response.");

        //Replace all unescaped quotes with escaped quotes
        messageContent = Regex.Replace(
            messageContent,
            // look-behind for "original":", "romanized":", or "translated":"
            // then capture any sequence (including already-escaped bits) up to the closing quote
            @"(?<=\b(?:original|romanized|translated)""\s*:\s*"")((?:\\.|[^""\\])*)(?="")",
            match =>
            {
                // in the captured value, escape any bare "
                string val = match.Value;
                string escaped = val.Replace("\"", "\\\"");
                return escaped;
            },
            RegexOptions.Singleline
        );
        try
        {
            TranslatePhrasesResponseDto? translateResponseDto = JsonSerializer.Deserialize<TranslatePhrasesResponseDto>(messageContent, _jsonOptions);
            if (translateResponseDto == null || translateResponseDto.Phrases.Length == 0)
            {
                _logger.LogError("No phrases were returned from the ChatGPT API for TranslateRomanize.");
                throw new InvalidOperationException("No phrases were returned from the ChatGPT API.");
            }
            return translateResponseDto;
        }
        catch (JsonException ex)
        {
            _logger.LogError(ex, "Failed to deserialize the ChatGPT API response for TranslateRomanize. messageContent: {MessageContent}", messageContent);
            throw new InvalidOperationException("Failed to deserialize the ChatGPT API response for TranslateRomanize.");
        }
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
            string escapedContent = await response.Content.ReadAsStringAsync();
            escapedContent = Regex.Replace(escapedContent, @"(?<!\\)`", @"");
            //catch slop word: json
            escapedContent = escapedContent.TrimStart();
            if (escapedContent.StartsWith("json", StringComparison.OrdinalIgnoreCase))
            {
                escapedContent = escapedContent[4..].TrimStart(); // Remove "json" and any spaces after
            }
            OpenAIResponseDto? openAIResponse = JsonSerializer.Deserialize<OpenAIResponseDto>(escapedContent, _jsonOptions);
            return openAIResponse;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error while calling the ChatGPT API.");
            throw new InvalidOperationException($"Caller: {caller}: Error while calling the ChatGPT API. {ex.Message}", ex);
        }
    }
}
