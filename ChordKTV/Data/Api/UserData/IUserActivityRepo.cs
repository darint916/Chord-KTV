using ChordKTV.Models.Playlist;
using ChordKTV.Models.Quiz;
using ChordKTV.Models.SongData;
using ChordKTV.Models.Handwriting;
using ChordKTV.Dtos;

namespace ChordKTV.Data.Api.UserData;

public interface IUserActivityRepo
{
    // Quiz related methods
    public Task<IEnumerable<UserQuizResult>> GetUserQuizResultsAsync(Guid userId, LanguageCode? language = null);
    public Task AddQuizResultAsync(UserQuizResult result);
    public Task ProcessQuizResultAsync(UserQuizResult result, IEnumerable<string> correctAnswers, LanguageCode language);

    // Handwriting related methods
    public Task<IEnumerable<UserHandwritingResult>> GetUserHandwritingResultsAsync(Guid userId, LanguageCode? language = null);
    public Task AddHandwritingResultAsync(UserHandwritingResult result);

    // Learned words related methods
    public Task<IEnumerable<LearnedWord>> GetUserLearnedWordsAsync(Guid userId, LanguageCode? language = null);
    public Task AddLearnedWordAsync(LearnedWord word);
    public Task SaveChangesAsync();

    // Song activity methods
    public Task<UserSongActivity?> GetUserSongActivityAsync(Guid userId, Guid songId);
    public Task<IEnumerable<UserSongActivity>> GetUserSongActivitiesAsync(Guid userId);
    public Task InsertSongActivityAsync(Guid userId, bool isFavorite, Song song);
    public Task UpsertSongActivityAsync(UserSongActivity activity, bool isPlayEvent = true);
    public Task<IEnumerable<UserSongActivity>> GetFavoriteSongActivitiesAsync(Guid userId);
    public Task UpdateSongActivityFavoriteAsync(UserSongActivity activity, bool isFavorite, bool isPlayed = false);

    // Playlist activity methods
    public Task<UserPlaylistActivity?> GetUserPlaylistActivityAsync(Guid userId, string playlistId);
    public Task InsertPlaylistActivityAsync(Guid userId, bool isFavorite, string playlistId, string playlistThumbnailUrl, string playlistTitle);
    public Task<IEnumerable<UserPlaylistActivity>> GetUserPlaylistActivitiesAsync(Guid userId);
    public Task UpsertPlaylistActivityAsync(UserPlaylistActivity activity, bool isPlayEvent = true);
    public Task<IEnumerable<UserPlaylistActivity>> GetFavoritePlaylistActivitiesAsync(Guid userId);

}
