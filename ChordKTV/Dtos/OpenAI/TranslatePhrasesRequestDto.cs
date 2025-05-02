namespace ChordKTV.Dtos.OpenAI;

public record TranslatePhrasesRequestDto
(
    string[] Phrases,
    LanguageCode LanguageCode,
    int Difficulty = 3
);
