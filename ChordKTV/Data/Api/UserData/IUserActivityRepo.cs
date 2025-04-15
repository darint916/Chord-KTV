using ChordKTV.Models.Playlist;
using ChordKTV.Models.Quiz;
using ChordKTV.Models.SongData;
using ChordKTV.Models.Handwriting;
using ChordKTV.Dtos;

namespace ChordKTV.Data.Api.UserData;

public interface IUserActivityRepo
{
    Task<UserPlaylistActivity?> GetPlaylistActivityAsync(Guid userId, string playlistUrl);
    Task<IEnumerable<UserQuizResult>> GetUserQuizResultsAsync(Guid userId);
    Task<IEnumerable<UserSongPlay>> GetUserSongPlaysAsync(Guid userId);
    Task<IEnumerable<UserHandwritingResult>> GetUserHandwritingResultsAsync(Guid userId);
    Task<IEnumerable<LearnedWord>> GetUserLearnedWordsAsync(Guid userId, LanguageCode? language = null);

    Task AddPlaylistActivityAsync(UserPlaylistActivity activity);
    Task AddQuizResultAsync(UserQuizResult result);
    Task AddSongPlayAsync(UserSongPlay play);
    Task AddHandwritingResultAsync(UserHandwritingResult result);
    Task AddLearnedWordAsync(LearnedWord word);

    Task<bool> SaveChangesAsync();

    Task<UserFavoriteSong?> GetFavoriteSongAsync(Guid userId, Guid songId);
    Task AddFavoriteSongAsync(UserFavoriteSong favoriteSong);
    Task RemoveFavoriteSongAsync(UserFavoriteSong favoriteSong);
    Task<IEnumerable<UserFavoriteSong>> GetUserFavoriteSongsAsync(Guid userId);

    Task<IEnumerable<UserPlaylistActivity>> GetUserPlaylistActivitiesAsync(Guid userId);

    Task<UserFavoritePlaylist?> GetFavoritePlaylistAsync(Guid userId, string playlistUrl);
    Task AddFavoritePlaylistAsync(UserFavoritePlaylist favoritePlaylist);
    Task RemoveFavoritePlaylistAsync(UserFavoritePlaylist favoritePlaylist);
    Task<IEnumerable<UserFavoritePlaylist>> GetUserFavoritePlaylistsAsync(Guid userId);
}
