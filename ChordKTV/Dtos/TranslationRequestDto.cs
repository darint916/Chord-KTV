namespace ChordKTV.Dtos;

public record TranslationRequestDto
(
    string OriginalLyrics,
    LanguageCode LanguageCode
);
