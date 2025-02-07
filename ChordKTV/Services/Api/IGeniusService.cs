namespace ChordKTV.Services.Api;

using ChordKTV.Dtos;

public interface IGeniusService
{
    Task<Models.SongData.Song?> GetSongByArtistTitleAsync(string title, string? artist);
    Task<List<Models.SongData.Song>> GetSongsByArtistTitleAsync(List<VideoInfo> videos);
    Task<Models.SongData.Song?> EnrichSongDetailsAsync(Models.SongData.Song song);
}
