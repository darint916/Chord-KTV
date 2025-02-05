using ChordKTV.Models.SongData;

namespace ChordKTV.Data.Api.SongData;
public interface IAlbumRepo
{
    public Task<bool> SaveChangesAsync();
    public Task AddAsync(Album album);
    public Task UpdateAsync(Album album);
    public Task<Album?> GetAlbumAsync(string name);
    public Task<Album?> GetAlbumAsync(string name, string artist);
    public Task<List<Album>> GetAllAlbumsAsync();
}