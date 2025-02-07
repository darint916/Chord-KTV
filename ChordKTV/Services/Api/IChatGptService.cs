

using ChordKTV.Dtos;
namespace ChordKTV.Services.Api;
public interface IChatGptService
{
    public Task<LrcLyricsDto> TranslateLyricsAsync(string originalLyrics, LanguageCode languageCode);
    public Task<List<LrcLyricsDto>> BatchTranslateLyricsAsync(List<TranslationRequestDto> lrcLyrics);
}
