using ChordKTV.Dtos.FullSong;

namespace ChordKTV.Services.Api;

public interface IFullSongService
{
    public Task<FullSongResponseDto?> GetFullSongAsync(string? title, string? artist, string? album, TimeSpan? duration, string? lyrics, string? youtubeId);
}
