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
    public async Task<IEnumerable<UserHandwritingResult>> GetUserHandwritingResultsAsync(Guid userId, LanguageCode? language = null)
    {
        IQueryable<UserHandwritingResult> query = _context.UserHandwritingResults.Where(x => x.UserId == userId);
        
        if (language.HasValue)
        {
            query = query.Where(x => x.Language == language.Value);
        }
        
        return await query
            .OrderByDescending(x => x.DateCompleted)
            .ToListAsync();
    }

    public async Task AddHandwritingResultAsync(UserHandwritingResult result)
    {
        await _context.UserHandwritingResults.AddAsync(result);
        await _context.SaveChangesAsync();
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
        await _context.SaveChangesAsync();
    }

    // Song Activity Methods
    public async Task<UserSongActivity?> GetUserSongActivityAsync(Guid userId, Guid songId)
    {
        return await _context.UserSongActivities
            .FirstOrDefaultAsync(x => x.UserId == userId && x.SongId == songId);
    }

    public async Task<IEnumerable<UserSongActivity>> GetUserSongActivitiesAsync(Guid userId)
    {
        return await _context.UserSongActivities
            .Where(x => x.UserId == userId)
            .OrderByDescending(x => x.LastPlayed)
            .ToListAsync();
    }

    public async Task UpsertSongActivityAsync(UserSongActivity activity, bool isPlayEvent = true)
    {
        UserSongActivity? existing = await GetUserSongActivityAsync(activity.UserId, activity.SongId);
        
        if (existing == null)
        {
            if (isPlayEvent)
            {
                activity.LastPlayed = DateTime.UtcNow;
                if (activity.DatesPlayed.Count == 0)
                {
                    activity.DatesPlayed.Add(activity.LastPlayed);
                }
            }
            
            activity.DateFavorited = activity.IsFavorite ? DateTime.UtcNow : null;
            await _context.UserSongActivities.AddAsync(activity);
        }
        else
        {
            if (isPlayEvent)
            {
                DateTime now = DateTime.UtcNow;
                existing.DatesPlayed.Add(now);
                existing.LastPlayed = now;
            }
            
            if (existing.IsFavorite != activity.IsFavorite)
            {
                existing.IsFavorite = activity.IsFavorite;
                existing.DateFavorited = activity.IsFavorite ? DateTime.UtcNow : null;
            }
        }
        
        await _context.SaveChangesAsync();
    }

    // Playlist Activity Methods
    public async Task<UserPlaylistActivity?> GetUserPlaylistActivityAsync(Guid userId, string playlistUrl)
    {
        return await _context.UserPlaylistActivities
            .FirstOrDefaultAsync(x => x.UserId == userId && x.PlaylistUrl == playlistUrl);
    }

    public async Task<IEnumerable<UserPlaylistActivity>> GetUserPlaylistActivitiesAsync(Guid userId)
    {
        return await _context.UserPlaylistActivities
            .Where(x => x.UserId == userId)
            .OrderByDescending(x => x.LastPlayed)
            .ToListAsync();
    }

    public async Task UpsertPlaylistActivityAsync(UserPlaylistActivity activity, bool isPlayEvent = true)
    {
        UserPlaylistActivity? existing = await GetUserPlaylistActivityAsync(activity.UserId, activity.PlaylistUrl);
        
        if (existing == null)
        {
            if (isPlayEvent)
            {
                activity.LastPlayed = DateTime.UtcNow;
                if (activity.DatesPlayed.Count == 0)
                {
                    activity.DatesPlayed.Add(activity.LastPlayed);
                }
            }
            
            activity.DateFavorited = activity.IsFavorite ? DateTime.UtcNow : null;
            await _context.UserPlaylistActivities.AddAsync(activity);
        }
        else
        {
            if (isPlayEvent)
            {
                DateTime now = DateTime.UtcNow;
                existing.DatesPlayed.Add(now);
                existing.LastPlayed = now;
            }
            
            if (existing.IsFavorite != activity.IsFavorite)
            {
                existing.IsFavorite = activity.IsFavorite;
                existing.DateFavorited = activity.IsFavorite ? DateTime.UtcNow : null;
            }
        }
        
        await _context.SaveChangesAsync();
    }

    public async Task ProcessQuizResultAsync(UserQuizResult result, IEnumerable<string> correctAnswers, LanguageCode language)
    {
        // Add quiz result
        await AddQuizResultAsync(result);

        // Process learned words
        var existingWords = await GetUserLearnedWordsAsync(result.UserId, language);
        var existingWordsSet = new HashSet<string>(existingWords.Select(lw => lw.Word));

        foreach (string word in correctAnswers.Where(w => !existingWordsSet.Contains(w)))
        {
            LearnedWord lw = new LearnedWord
            {
                UserId = result.UserId,
                Word = word,
                Language = language,
                DateLearned = DateTime.UtcNow
            };
            await AddLearnedWordAsync(lw);
        }
        
        await _context.SaveChangesAsync();
    }

    public async Task<IEnumerable<UserSongActivity>> GetFavoriteSongActivitiesAsync(Guid userId)
    {
        return await _context.UserSongActivities
            .Where(x => x.UserId == userId && x.IsFavorite)
            .OrderByDescending(x => x.LastPlayed)
            .ToListAsync();
    }

    public async Task<IEnumerable<UserPlaylistActivity>> GetFavoritePlaylistActivitiesAsync(Guid userId)
    {
        return await _context.UserPlaylistActivities
            .Where(x => x.UserId == userId && x.IsFavorite)
            .OrderByDescending(x => x.LastPlayed)
            .ToListAsync();
    }
}
