using ChordKTV.Models.SongData;

namespace ChordKTV.Data.Api.SongData;

public interface ISongRepo
{
    public Task<bool> SaveChangesAsync();
    public Task AddSongAsync(Song song);
    public Task UpdateSongAsync(Song song);
    public Task<Song?> GetSongAsync(string name);
    public Task<Song?> GetSongAsync(string name, string artist);
    public Task<Song?> GetSongAsync(string name, string artist, string albumName);
    public Task<List<Song>> GetAllSongsAsync();
    public Task<Song?> GetSongByIdAsync(Guid id);
    public Task<Song?> GetSongByGeniusIdAsync(int geniusId);
    public Task<Song?> GetSongByLrcIdAsync(int lrcId);
    public Task<Song?> GetSongByRomanizedLrcIdAsync(int romLrcId);
}
