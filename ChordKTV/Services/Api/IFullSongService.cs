using ChordKTV.Models.SongData;

namespace ChordKTV.Services.Api;

public interface IFullSongService
{
    public Task<Song?> GetFullSongAsync(string? title, string? artist, string? album, TimeSpan? duration, string? lyrics, string? youtubeId);
}
