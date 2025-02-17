namespace ChordKTV.Dtos.TranslationGptApi;

public record TranslationRequestDto
(
    string OriginalLyrics,
    LanguageCode LanguageCode,
    bool Romanize,
    bool Translate
);
