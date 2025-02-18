namespace ChordKTV.Dtos.TranslationGptApi;

public class TranslationResponseDto
{
    public required string OriginalLyrics { get; set; }
    public LanguageCode LanguageCode { get; set; }
    public string? RomanizedLyrics { get; set; }
    public string? TranslatedLyrics { get; set; }
}
