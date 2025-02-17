using ChordKTV.Dtos;
using ChordKTV.Dtos.TranslationGptApi;

namespace ChordKTV.Services.Api;

public interface IChatGptService
{
    public Task<TranslationResponseDto> TranslateLyricsAsync(string originalLyrics, LanguageCode languageCode, bool romanize, bool translate);
    public Task<List<TranslationResponseDto>> BatchTranslateLyricsAsync(List<TranslationRequestDto> lrcLyrics);
}
