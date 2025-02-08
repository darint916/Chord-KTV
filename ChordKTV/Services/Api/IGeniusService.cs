namespace ChordKTV.Services.Api;

using ChordKTV.Dtos;
using ChordKTV.Models.SongData;

public interface IGeniusService
{
    Task<Song?> GetSongByArtistTitleAsync(string title, string? artist);
    Task<List<Song>> GetSongsByArtistTitleAsync(List<VideoInfo> videos);
    Task<Song?> EnrichSongDetailsAsync(Song song);
}
