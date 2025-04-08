using ChordKTV.Models.Playlist;
using ChordKTV.Models.Quiz;
using ChordKTV.Models.SongData;
using ChordKTV.Models.Handwriting;

namespace ChordKTV.Data.Api.UserData;

public interface IUserActivityRepo
{
    Task<UserPlaylistActivity?> GetPlaylistActivityAsync(Guid userId, string playlistUrl);
    Task<IEnumerable<UserQuizResult>> GetUserQuizResultsAsync(Guid userId);
    Task<IEnumerable<UserSongPlay>> GetUserSongPlaysAsync(Guid userId);
    Task<IEnumerable<UserHandwritingResult>> GetUserHandwritingResultsAsync(Guid userId);
    Task<IEnumerable<LearnedWord>> GetUserLearnedWordsAsync(Guid userId, string? language = null);
    
    Task AddPlaylistActivityAsync(UserPlaylistActivity activity);
    Task AddQuizResultAsync(UserQuizResult result);
    Task AddSongPlayAsync(UserSongPlay play);
    Task AddHandwritingResultAsync(UserHandwritingResult result);
    Task AddLearnedWordAsync(LearnedWord word);
    
    Task<bool> SaveChangesAsync();
} 