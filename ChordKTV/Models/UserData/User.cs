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
    public ICollection<UserSongActivity> SongActivities { get; set; } = new List<UserSongActivity>();
    public ICollection<UserHandwritingResult> HandwritingResults { get; set; } = new List<UserHandwritingResult>();
    public ICollection<LearnedWord> LearnedWords { get; set; } = new List<LearnedWord>();
}
