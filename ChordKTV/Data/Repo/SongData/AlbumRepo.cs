using ChordKTV.Data.Api.SongData;
using ChordKTV.Models.SongData;
using Microsoft.EntityFrameworkCore;

namespace ChordKTV.Data.Repo.SongData;

public class AlbumRepo : IAlbumRepo
{
    private readonly AppDbContext _context;

    public AlbumRepo(AppDbContext context)
    {
        _context = context;
    }

    public async Task<bool> SaveChangesAsync()
    {
        return await _context.SaveChangesAsync() > 0;
    }

    public async Task AddAsync(Album album)
    {
        _context.Albums.Add(album);
        await _context.SaveChangesAsync();
    }

    public async Task UpdateAsync(Album album)
    {
        _context.Albums.Update(album);
        await _context.SaveChangesAsync();
    }

    // Note these are exact matches, due to copying from genius
    // If we want client search db use .Contains for similar matching (slower).
    public async Task<Album?> GetAlbumAsync(string name)
    {
        return await _context.Albums.FirstOrDefaultAsync(a => a.Name == name);
    }

    public async Task<Album?> GetAlbumAsync(string name, string artist)
    {
        return await _context.Albums.FirstOrDefaultAsync(a => a.Name == name && a.Artist == artist);
    }

    public async Task<List<Album>> GetAllAlbumsAsync()
    {
        return await _context.Albums.ToListAsync();
    }
}
