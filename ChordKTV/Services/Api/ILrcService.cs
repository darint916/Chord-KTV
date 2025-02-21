using ChordKTV.Dtos;

namespace ChordKTV.Services.Api;

public interface ILrcService
{
    public Task<LrcLyricsDto?> GetAllLrcLibLyricsAsync(string? title, string? artist, string? albumName, float? duration);
}
