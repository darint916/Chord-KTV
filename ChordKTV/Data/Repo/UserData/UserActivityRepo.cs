using ChordKTV.Data.Api.UserData;
using ChordKTV.Models.UserData;
using ChordKTV.Models.Quiz;
using ChordKTV.Models.Playlist;
using ChordKTV.Models.SongData;
using ChordKTV.Models.Handwriting;
using ChordKTV.Dtos;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace ChordKTV.Data.Repo.UserData;

public class UserActivityRepo : IUserActivityRepo
{
    private readonly AppDbContext _context;

    public UserActivityRepo(AppDbContext context)
    {
        _context = context;
    }

    // Quiz Methods
    public async Task<IEnumerable<UserQuizResult>> GetUserQuizResultsAsync(Guid userId)
    {
        return await _context.UserQuizzesDone
            .Where(x => x.UserId == userId)
            .OrderByDescending(x => x.DateCompleted)
            .ToListAsync();
    }

    public async Task AddQuizResultAsync(UserQuizResult result)
    {
        await _context.UserQuizzesDone.AddAsync(result);
    }

    // Handwriting Methods
    public async Task<IEnumerable<UserHandwritingResult>> GetUserHandwritingResultsAsync(Guid userId)
    {
        return await _context.UserHandwritingResults
            .Where(x => x.UserId == userId)
            .OrderByDescending(x => x.DateCompleted)
            .ToListAsync();
    }

    public async Task AddHandwritingResultAsync(UserHandwritingResult result)
    {
        await _context.UserHandwritingResults.AddAsync(result);
    }

    // Learned Words Methods
    public async Task<IEnumerable<LearnedWord>> GetUserLearnedWordsAsync(Guid userId, LanguageCode? language = null)
    {
        IQueryable<LearnedWord> query = _context.LearnedWords.Where(x => x.UserId == userId);
        if (language.HasValue)
        {
            query = query.Where(x => x.Language == language.Value);
        }
        return await query.OrderByDescending(x => x.DateLearned).ToListAsync();
    }

    public async Task AddLearnedWordAsync(LearnedWord word)
    {
        await _context.LearnedWords.AddAsync(word);
    }

    // Song Activity Methods (Combined)
    public async Task<UserSongActivity?> GetUserSongActivityAsync(Guid userId, Guid songId)
    {
        return await _context.UserSongActivities
            .FirstOrDefaultAsync(x => x.UserId == userId && x.SongId == songId);
    }

    public async Task<IEnumerable<UserSongActivity>> GetUserSongActivitiesAsync(Guid userId)
    {
        return await _context.UserSongActivities
            .Where(x => x.UserId == userId)
            .OrderByDescending(x => x.DatesPlayed.Max())
            .ToListAsync();
    }

    public async Task UpsertSongActivityAsync(UserSongActivity activity)
    {
        UserSongActivity? existing = await GetUserSongActivityAsync(activity.UserId, activity.SongId);
        if (existing == null)
        {
            await _context.UserSongActivities.AddAsync(activity);
        }
        else
        {
            existing.DatesPlayed = activity.DatesPlayed;
            existing.IsFavorite = activity.IsFavorite;
        }
    }

    // Playlist Activity Methods (Combined)
    public async Task<UserPlaylistActivity?> GetUserPlaylistActivityAsync(Guid userId, string playlistUrl)
    {
        return await _context.UserPlaylistActivities
            .FirstOrDefaultAsync(x => x.UserId == userId && x.PlaylistUrl == playlistUrl);
    }

    public async Task<IEnumerable<UserPlaylistActivity>> GetUserPlaylistActivitiesAsync(Guid userId)
    {
        return await _context.UserPlaylistActivities
            .Where(x => x.UserId == userId)
            .OrderByDescending(x => x.DatesPlayed.Max())
            .ToListAsync();
    }

    public async Task UpsertPlaylistActivityAsync(UserPlaylistActivity activity)
    {
        UserPlaylistActivity? existing = await GetUserPlaylistActivityAsync(activity.UserId, activity.PlaylistUrl);
        if (existing == null)
        {
            await _context.UserPlaylistActivities.AddAsync(activity);
        }
        else
        {
            existing.DatesPlayed = activity.DatesPlayed;
            existing.IsFavorite = activity.IsFavorite;
        }
    }

    public async Task<bool> SaveChangesAsync()
    {
        return await _context.SaveChangesAsync() > 0;
    }
}
