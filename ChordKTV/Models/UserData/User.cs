using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using ChordKTV.Models.Playlist;
using ChordKTV.Models.Quiz;
using ChordKTV.Models.SongData;
using ChordKTV.Models.Handwriting;

namespace ChordKTV.Models.UserData;

public class User
{
    [Key]
    public Guid Id { get; set; } = Guid.NewGuid();
    public required string Name { get; set; }
    public required string Email { get; set; }

    public ICollection<UserPlaylistActivity> PlaylistActivities { get; set; } = new List<UserPlaylistActivity>();
    public ICollection<UserQuizResult> QuizResults { get; set; } = new List<UserQuizResult>();
    public ICollection<UserSongPlay> SongPlays { get; set; } = new List<UserSongPlay>();
    public ICollection<UserHandwritingResult> HandwritingResults { get; set; } = new List<UserHandwritingResult>();
    public ICollection<UserFavoriteSong> FavoriteSongs { get; set; } = new List<UserFavoriteSong>();
    public ICollection<LearnedWord> LearnedWords { get; set; } = new List<LearnedWord>();
    public ICollection<UserFavoritePlaylist> FavoritePlaylists { get; set; } = new List<UserFavoritePlaylist>();
}
