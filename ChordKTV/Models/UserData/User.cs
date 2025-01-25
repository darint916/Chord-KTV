using System.ComponentModel.DataAnnotations;

namespace ChordKTV.Models.UserData;

public class User
{
    [Key]
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    //public string PasswordHash { get; set; } = string.Empty;
    public List<string> FavoriteSongs { get; set; } = new List<string>();
    public List<string> FavoriteArtists { get; set; } = new List<string>();
    public List<string> FavoriteAlbums { get; set; } = new List<string>();
    public List<string> FavoritePlaylistLinks { get; set; } = new List<string>();
    public List<string> SongHistory { get; set; } = new List<string>();
    public List<string> PlaylistHistory { get; set; } = new List<string>();
}