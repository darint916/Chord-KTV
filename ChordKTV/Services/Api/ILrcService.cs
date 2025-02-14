using ChordKTV.Dtos;

namespace ChordKTV.Services.Api;

public interface ILrcService
{
    // Added "qString" which LRCLIB uses to search for keyword present in ANY fields (track's title, artist name or album name)
    public Task<LrcLyricsDto?> GetLrcLibLyricsAsync(string? title, string? artist, string? albumName, string? qString, float? duration);
}
