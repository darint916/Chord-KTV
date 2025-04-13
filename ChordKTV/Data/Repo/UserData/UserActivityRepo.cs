using ChordKTV.Data.Api.UserData;
using ChordKTV.Models.UserData;
using ChordKTV.Models.Quiz;
using ChordKTV.Models.Playlist;
using ChordKTV.Models.SongData;
using ChordKTV.Models.Handwriting;
using Microsoft.EntityFrameworkCore;

namespace ChordKTV.Data.Repo.UserData;

public class UserActivityRepo : IUserActivityRepo
{
    private readonly AppDbContext _context;

    public UserActivityRepo(AppDbContext context, ILogger<UserActivityRepo> logger)
    {
        _context = context;
    }

    public async Task<UserPlaylistActivity?> GetPlaylistActivityAsync(Guid userId, string playlistUrl)
    {
        return await _context.UserPlaylistActivities
            .FirstOrDefaultAsync(x => x.UserId == userId && x.PlaylistUrl == playlistUrl);
    }

    public async Task<IEnumerable<UserQuizResult>> GetUserQuizResultsAsync(Guid userId)
    {
        return await _context.UserQuizzesDone
            .Where(x => x.UserId == userId)
            .OrderByDescending(x => x.CompletedAt)
            .ToListAsync();
    }

    public async Task<IEnumerable<UserSongPlay>> GetUserSongPlaysAsync(Guid userId)
    {
        return await _context.UserSongPlays
            .Where(x => x.UserId == userId)
            .OrderByDescending(x => x.PlayedAt)
            .ToListAsync();
    }

    public async Task<IEnumerable<UserHandwritingResult>> GetUserHandwritingResultsAsync(Guid userId)
    {
        return await _context.UserHandwritingResults
            .Where(x => x.UserId == userId)
            .OrderByDescending(x => x.CompletedAt)
            .ToListAsync();
    }

    public async Task<IEnumerable<LearnedWord>> GetUserLearnedWordsAsync(Guid userId, string? language = null)
    {
        IQueryable<LearnedWord> query = _context.LearnedWords.Where(x => x.UserId == userId);

        if (!string.IsNullOrEmpty(language))
        {
            query = query.Where(x => x.Language == language);
        }

        return await query.OrderByDescending(x => x.LearnedOn).ToListAsync();
    }

    public async Task AddPlaylistActivityAsync(UserPlaylistActivity activity)
    {
        await _context.UserPlaylistActivities.AddAsync(activity);
    }

    public async Task AddQuizResultAsync(UserQuizResult result)
    {
        await _context.UserQuizzesDone.AddAsync(result);
    }

    public async Task AddSongPlayAsync(UserSongPlay play)
    {
        await _context.UserSongPlays.AddAsync(play);
    }

    public async Task AddHandwritingResultAsync(UserHandwritingResult result)
    {
        await _context.UserHandwritingResults.AddAsync(result);
    }

    public async Task AddLearnedWordAsync(LearnedWord word)
    {
        await _context.LearnedWords.AddAsync(word);
    }

    public async Task<bool> SaveChangesAsync()
    {
        return await _context.SaveChangesAsync() > 0;
    }

    public async Task<UserFavoriteSong?> GetFavoriteSongAsync(Guid userId, Guid songId)
    {
        return await _context.FavoriteSongs
            .FirstOrDefaultAsync(x => x.UserId == userId && x.SongId == songId);
    }

    public async Task<IEnumerable<UserFavoriteSong>> GetUserFavoriteSongsAsync(Guid userId)
    {
        return await _context.FavoriteSongs
            .Where(x => x.UserId == userId)
            .OrderByDescending(x => x.FavoritedAt)
            .ToListAsync();
    }

    public async Task AddFavoriteSongAsync(UserFavoriteSong favoriteSong)
    {
        await _context.FavoriteSongs.AddAsync(favoriteSong);
    }

    public Task RemoveFavoriteSongAsync(UserFavoriteSong favoriteSong)
    {
        _context.FavoriteSongs.Remove(favoriteSong);
        return Task.CompletedTask;
    }

    public async Task<IEnumerable<UserPlaylistActivity>> GetUserPlaylistActivitiesAsync(Guid userId)
    {
        return await _context.UserPlaylistActivities
            .Where(x => x.UserId == userId)
            .OrderByDescending(x => x.LastPlayed)
            .ToListAsync();
    }

    public async Task<UserFavoritePlaylist?> GetFavoritePlaylistAsync(Guid userId, string playlistUrl)
    {
        return await _context.FavoritePlaylists
            .FirstOrDefaultAsync(x => x.UserId == userId && x.PlaylistUrl == playlistUrl);
    }

    public async Task<IEnumerable<UserFavoritePlaylist>> GetUserFavoritePlaylistsAsync(Guid userId)
    {
        return await _context.FavoritePlaylists
            .Where(x => x.UserId == userId)
            .OrderByDescending(x => x.FavoritedAt)
            .ToListAsync();
    }

    public async Task AddFavoritePlaylistAsync(UserFavoritePlaylist favoritePlaylist)
    {
        await _context.FavoritePlaylists.AddAsync(favoritePlaylist);
    }

    public Task RemoveFavoritePlaylistAsync(UserFavoritePlaylist favoritePlaylist)
    {
        _context.FavoritePlaylists.Remove(favoritePlaylist);
        return Task.CompletedTask;
    }
}
