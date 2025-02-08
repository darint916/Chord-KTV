using ChordKTV.Data.Api.SongData;
using ChordKTV.Models.SongData;
using Microsoft.EntityFrameworkCore;
namespace ChordKTV.Data.Repo;


public class SongRepo : ISongRepo
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
        string normalizedName = name.Trim().ToLower();
        return await _context.Songs
            .Include(s => s.GeniusMetaData)
            .Include(s => s.Albums)
            .FirstOrDefaultAsync(s => 
                s.Name.Trim().ToLower() == normalizedName || 
                s.AlternateNames.Any(n => n.Trim().ToLower() == normalizedName)
            );
    }

    // Will null if artist isnt direct match, as we expect direct artist match (assuming this is from genius data)
    public async Task<Song?> GetSongAsync(string name, string artist)
    {
        string normalizedName = name.Trim().ToLower();
        string normalizedArtist = artist.Trim().ToLower();
        
        return await _context.Songs
            .Include(s => s.GeniusMetaData)
            .Include(s => s.Albums)
            .FirstOrDefaultAsync(s => 
                s.Name.Trim().ToLower() == normalizedName && 
                ((s.PrimaryArtist.Trim().ToLower() == normalizedArtist) || 
                 s.FeaturedArtists.Any(a => a.Trim().ToLower() == normalizedArtist))
            );
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
