using ChordKTV.Models.SongData;

namespace ChordKTV.Data.Api.SongData;

public interface ISongRepo
{
    public Task<bool> SaveChangesAsync();
    public Task AddAsync(Song song);
    public Task UpdateAsync(Song song);
    public Task<Song?> GetSongAsync(string name);
    public Task<Song?> GetSongAsync(string name, string artist);
    public Task<Song?> GetSongAsync(string name, string artist, string albumName);
    public Task<List<Song>> GetAllSongsAsync();
    public Task<GeniusMetaData?> GetGeniusMetaDataAsync(int geniusId);
}
