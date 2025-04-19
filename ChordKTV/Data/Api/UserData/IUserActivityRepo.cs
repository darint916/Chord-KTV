using ChordKTV.Models.Playlist;
using ChordKTV.Models.Quiz;
using ChordKTV.Models.SongData;
using ChordKTV.Models.Handwriting;
using ChordKTV.Dtos;

namespace ChordKTV.Data.Api.UserData;

public interface IUserActivityRepo
{
    // Quiz related methods
    Task<IEnumerable<UserQuizResult>> GetUserQuizResultsAsync(Guid userId);
    Task AddQuizResultAsync(UserQuizResult result);

    // Handwriting related methods
    Task<IEnumerable<UserHandwritingResult>> GetUserHandwritingResultsAsync(Guid userId);
    Task AddHandwritingResultAsync(UserHandwritingResult result);

    // Learned words related methods
    Task<IEnumerable<LearnedWord>> GetUserLearnedWordsAsync(Guid userId, LanguageCode? language = null);
    Task AddLearnedWordAsync(LearnedWord word);

    // Song activity methods
    Task<UserSongActivity?> GetUserSongActivityAsync(Guid userId, Guid songId);
    Task<IEnumerable<UserSongActivity>> GetUserSongActivitiesAsync(Guid userId);
    Task UpsertSongActivityAsync(UserSongActivity activity);

    // Playlist activity methods
    Task<UserPlaylistActivity?> GetUserPlaylistActivityAsync(Guid userId, string playlistUrl);
    Task<IEnumerable<UserPlaylistActivity>> GetUserPlaylistActivitiesAsync(Guid userId);
    Task UpsertPlaylistActivityAsync(UserPlaylistActivity activity);

    Task<bool> SaveChangesAsync();
}
