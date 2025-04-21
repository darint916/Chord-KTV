using ChordKTV.Dtos;
using ChordKTV.Dtos.GeniusApi;
using ChordKTV.Models.SongData;

namespace ChordKTV.Services.Api;

public interface IGeniusService
{
    public Task<Song?> GetSongByArtistTitleAsync(string? title, string? artist, string? lyrics, bool forceRefresh = false);
    public Task<List<Song>> GetSongsByArtistTitleAsync(List<VideoInfo> videos, bool forceRefresh = false);
    public Task<Song> EnrichSongDetailsAsync(Song song);
    public Task<List<GeniusHit>?> GetGeniusSearchResultsAsync(string searchQuery);
}
