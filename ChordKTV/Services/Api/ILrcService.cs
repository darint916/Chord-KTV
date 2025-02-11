using ChordKTV.Dtos;

namespace ChordKTV.Services.Api;

public interface ILrcService
{
    public Task<LrcLyricsDto?> GetLrcLibLyricsAsync(string title, string artist, string? albumName, float? duration);

    public Task<LrcLyricsDto?> GetLrcRomanizedLyricsAsync(LrcLyricsDto lyricsDto);
}
