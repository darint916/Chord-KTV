namespace ChordKTV.Models.UserData;
using System.ComponentModel.DataAnnotations;

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
}
