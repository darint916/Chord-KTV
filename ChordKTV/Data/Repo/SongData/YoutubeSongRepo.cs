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

    public async Task<YoutubeSong?> GetYoutubeSongByYoutubeIdAsync(string id)
    {
        return await _context.YoutubeSongs
            .Include(s => s.Song)
            .ThenInclude(s => s.GeniusMetaData)
            .Include(s => s.Song.Albums)
            .FirstOrDefaultAsync(s => s.YoutubeId == id);
    }
    public async Task<Song?> GetSongByYoutubeIdAsync(string id)
    {
        YoutubeSong? youtubeSong = await _context.YoutubeSongs
            .Include(s => s.Song)
            .ThenInclude(s => s.GeniusMetaData)
            .Include(s => s.Song.Albums)
            .FirstOrDefaultAsync(s => s.YoutubeId == id);
        return youtubeSong?.Song;
    }

    public async Task AddYoutubeSongAsync(YoutubeSong youtubeSong)
    {
        if (string.IsNullOrWhiteSpace(youtubeSong.YoutubeId))
        {
            throw new ArgumentException("YoutubeSongRepo: YoutubeId cannot be null or whitespace.");
        }
        _context.YoutubeSongs.Add(youtubeSong);
        await _context.SaveChangesAsync();
    }
}
