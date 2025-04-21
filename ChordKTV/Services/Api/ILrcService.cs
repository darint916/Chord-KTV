using ChordKTV.Dtos.LrcLib;

namespace ChordKTV.Services.Api;

public interface ILrcService
{
    public Task<LrcLyricsDto?> GetAllLrcLibLyricsAsync(string? title, string? artist, string? albumName, float? duration);
    public Task<List<LrcLyricsDto>?> GetLrcLibSearchResultsAsync(string searchQuery);
}
