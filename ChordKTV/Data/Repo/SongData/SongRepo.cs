
using ChordKTV.Models.SongData;
using Microsoft.EntityFrameworkCore;
namespace ChordKTV.Data.Repo;


public class SongRepo
{
    private readonly AppDbContext _context;

    public SongRepo(AppDbContext context)
    {
        _context = context;
    }

    public async Task<bool> SaveChangesAsync()
    {
        return await _context.SaveChangesAsync() > 0;
    }

    public async Task AddAsync(Song song)
    {
        _context.Songs.Add(song);
        await _context.SaveChangesAsync();
    }

    public async Task UpdateAsync(Song song)
    {
        _context.Songs.Update(song);
        await _context.SaveChangesAsync();
    }

    public async Task<Song?> GetSongAsync(string name)
    {
        Song? song = await _context.Songs.FirstOrDefaultAsync(s => s.Name == name) ??
            await _context.Songs.FirstOrDefaultAsync(s => s.AlternateNames.Contains(name));
        return song;
    }

    // Will null if artist isnt direct match, as we expect direct artist match (assuming this is from genius data)
    public async Task<Song?> GetSongAsync(string name, string artist)
    {
        return await _context.Songs.FirstOrDefaultAsync(s => s.Name == name && ((s.PrimaryArtist == artist) || s.FeaturedArtists.Contains(artist)));
    }


    public async Task<Song?> GetSongAsync(string name, string artist, string albumName)
    {
        return await _context.Songs.FirstOrDefaultAsync(s =>
            (s.Name == name || s.AlternateNames.Contains(name)) &&
            ((s.PrimaryArtist == artist) || s.FeaturedArtists.Contains(artist)) &&
            s.Albums.Any(album => album.Name == albumName));
    }

    public async Task<List<Song>> GetAllSongsAsync()
    {
        return await _context.Songs.ToListAsync();
    }

}
