using System.ComponentModel.DataAnnotations;
using ChordKTV.Models.Playlist;
using ChordKTV.Models.Quiz;
using ChordKTV.Models.SongData;
using ChordKTV.Models.Handwriting;

namespace ChordKTV.Models.UserData;

public class User
{
    [Key]
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    //public string PasswordHash { get; set; } = string.Empty;
    public List<string> FavoriteSongs { get; set; } = [];
    public List<string> FavoriteArtists { get; set; } = [];
    public List<string> FavoriteAlbums { get; set; } = [];
    public List<string> FavoritePlaylistLinks { get; set; } = [];
    public List<string> SongHistory { get; set; } = [];
    public List<string> PlaylistHistory { get; set; } = [];

    // Navigation properties with updated namespaces
    public ICollection<UserPlaylistActivity> PlaylistActivities { get; set; } = [];
    public ICollection<UserQuizResult> QuizResults { get; set; } = [];
    public ICollection<UserSongPlay> SongPlays { get; set; } = [];
    public ICollection<UserHandwritingResult> HandwritingResults { get; set; } = [];
    public ICollection<LearnedWord> LearnedWords { get; set; } = [];
}
