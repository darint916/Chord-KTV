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

    public List<UserPlaylistActivity> PlaylistActivities { get; set; } = new List<UserPlaylistActivity>();
    public List<UserQuizResult> QuizResults { get; set; } = new List<UserQuizResult>();
    public List<UserSongActivity> SongActivities { get; set; } = new List<UserSongActivity>();
    public List<UserHandwritingResult> HandwritingResults { get; set; } = new List<UserHandwritingResult>();
    public List<LearnedWord> LearnedWords { get; set; } = new List<LearnedWord>();

    public string? RefreshToken { get; set; }
    public DateTime? RefreshTokenExpiry { get; set; }
}
