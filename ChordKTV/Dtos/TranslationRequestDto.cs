namespace ChordKTV.Dtos;

public record TranslationRequestDto
(
    string OriginalLyrics,
    LanguageCode LanguageCode,
    bool Romanize,
    bool Translate
);
