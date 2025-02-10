namespace ChordKTV.Services.Api;

using ChordKTV.Dtos;

public interface ILrcService
{
    public Task<LrcLyricsDto?> GetLrcLibLyricsAsync(string title, string artist, string? albumName, float? duration);

    public Task<LrcLyricsDto?> GetLrcRomanizedLyricsAsync(LrcLyricsDto lyricsDto);
}
