using ChordKTV.Data.Api.SongData;
using ChordKTV.Models.SongData;
using Microsoft.EntityFrameworkCore;
namespace ChordKTV.Data.Repo;


public class SongRepo : ISongRepo
{
    private readonly AppDbContext _context;
    private readonly ILogger<SongRepo> _logger;

    public SongRepo(AppDbContext context, ILogger<SongRepo> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<bool> SaveChangesAsync()
    {
        return await _context.SaveChangesAsync() > 0;
    }

    public async Task AddSongAsync(Song song)
    {
        _context.Songs.Add(song);
        await _context.SaveChangesAsync();
    }

    public async Task UpdateSongAsync(Song song)
    {
        _context.Songs.Update(song);
        await _context.SaveChangesAsync();
    }

    public async Task<Song?> GetSongAsync(string name)
    {
        // Normalize input on the client side (remove spaces)
        string normalizedName = name.Trim().Replace(" ", "");

        // Retrieve songs from the database first
        List<Song> songs = await _context.Songs
            .Include(s => s.GeniusMetaData)
            .Include(s => s.Albums)
            .ToListAsync();

        // Use a case-insensitive comparison with StringComparison.OrdinalIgnoreCase
        return songs.FirstOrDefault(s =>
            string.Equals(s.Title.Trim().Replace(" ", ""), normalizedName, StringComparison.OrdinalIgnoreCase) ||
            s.AlternateTitles.Any(n => string.Equals(n.Trim().Replace(" ", ""), normalizedName, StringComparison.OrdinalIgnoreCase))
        );
    }

    // Will null if artist isnt direct match, as we expect direct artist match (assuming this is from genius data)
    public async Task<Song?> GetSongAsync(string name, string artist)
    {
        // Normalize input on the client side (remove spaces; leave case alone)
        string normalizedName = name.Trim().Replace(" ", "");
        string normalizedArtist = artist.Trim();

        _logger.LogDebug("Looking up song in cache. Name: {Name}, Artist: {Artist}", normalizedName, normalizedArtist);

        // Retrieve songs from the database first
        List<Song> songs = await _context.Songs
            .Include(s => s.GeniusMetaData)
            .Include(s => s.Albums)
            .ToListAsync();

        // Use string.Equals(..., StringComparison.OrdinalIgnoreCase) and the StartsWith overload with StringComparison
        Song? result = songs.FirstOrDefault(s =>
            (string.Equals(s.Title.Trim().Replace(" ", ""), normalizedName, StringComparison.OrdinalIgnoreCase) ||
             s.AlternateTitles.Any(n =>
                 string.Equals(n.Trim().Replace(" ", ""), normalizedName, StringComparison.OrdinalIgnoreCase))) &&
            (s.Artist.Trim().StartsWith(normalizedArtist, StringComparison.OrdinalIgnoreCase) ||
             s.FeaturedArtists.Any(a => a.Trim().StartsWith(normalizedArtist, StringComparison.OrdinalIgnoreCase)))
        );

        _logger.LogDebug("Cache lookup result: {Result}", result != null ? "Found" : "Not found");

        return result;
    }

    public async Task<Song?> GetSongAsync(string name, string artist, string albumName)
    {
        return await _context.Songs.FirstOrDefaultAsync(s =>
            (s.Title == name || s.AlternateTitles.Contains(name)) &&
            ((s.Artist == artist) || s.FeaturedArtists.Contains(artist)) &&
            s.Albums.Any(album => album.Name == albumName));
    }

    public async Task<List<Song>> GetAllSongsAsync()
    {
        return await _context.Songs.ToListAsync();
    }

    public async Task<GeniusMetaData?> GetGeniusMetaDataAsync(int geniusId)
    {
        return await _context.GeniusMetaData.FindAsync(geniusId);
    }

    public async Task<Song?> GetSongByGeniusIdAsync(int geniusId)
    {
        return await _context.Songs
            .Include(s => s.GeniusMetaData)
            .FirstOrDefaultAsync(s => s.GeniusMetaData.GeniusId == geniusId);
    }
}
