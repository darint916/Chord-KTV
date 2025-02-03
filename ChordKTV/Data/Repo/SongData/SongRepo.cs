
using ChordKTV.Models.SongData;
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

    public async Task<Song?> GetSongAsync(string name, string? artist, string? album)
    {
        Song? song = await _context.Songs.FirstOrDefaultAsync(s => s.Name == name && s.Artist == artist && s.Album == album);
    }
}