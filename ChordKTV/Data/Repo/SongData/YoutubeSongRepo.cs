using ChordKTV.Data.Api.SongData;
using ChordKTV.Models.SongData;
using Microsoft.EntityFrameworkCore;

namespace ChordKTV.Data.Repo.SongData;

public class YoutubeSongRepo : IYoutubeSongRepo
{
    private readonly AppDbContext _context;
    public YoutubeSongRepo(AppDbContext context)
    {
        _context = context;
    }

    public async Task<YoutubeSong?> GetYoutubeSongByYoutubeId(string id)
    {
        return await _context.YoutubeSongs
            .Include(s => s.Song)
            .ThenInclude(s => s.GeniusMetaData)
            .Include(s => s.Song.Albums)
            .FirstOrDefaultAsync(s => s.YoutubeId == id);
    }
    public async Task<Song?> GetSongByYoutubeId(string id)
    {
        YoutubeSong? youtubeSong = await _context.YoutubeSongs
            .Include(s => s.Song)
            .ThenInclude(s => s.GeniusMetaData)
            .Include(s => s.Song.Albums)
            .FirstOrDefaultAsync(s => s.YoutubeId == id);
        return youtubeSong?.Song;
    }

    public async Task AddYoutubeSongAsync(YoutubeSong song)
    {
        _context.YoutubeSongs.Add(song);
        await _context.SaveChangesAsync();
    }
}
