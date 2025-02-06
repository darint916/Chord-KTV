

using ChordKTV.Dtos;
namespace ChordKTV.Services.Api;
public interface IChatGptService
{
    public Task<LrcLyricsDto> TranslateLyricsAsync(LrcLyricsDto lrcLyrics);
    public Task<List<LrcLyricsDto>> BatchTranslateLyricsAsync(List<LrcLyricsDto> lrcLyrics);
}
