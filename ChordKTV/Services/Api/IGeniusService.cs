namespace ChordKTV.Services.Api;

using ChordKTV.Dtos;
using ChordKTV.Models.SongData;

public interface IGeniusService
{
    Task<Song?> GetSongByArtistTitleAsync(string title, string? artist, bool forceRefresh = false);
    Task<List<Song>> GetSongsByArtistTitleAsync(List<VideoInfo> videos, bool forceRefresh = false);
    Task<Song?> EnrichSongDetailsAsync(Song song);
}
