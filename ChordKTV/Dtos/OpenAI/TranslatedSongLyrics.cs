namespace ChordKTV.Dtos.OpenAI;
public record TranslatedSongLyrics
(
    string? RomanizedLyrics,
    string? TranslatedLyrics,
    string LanguageCode
);
