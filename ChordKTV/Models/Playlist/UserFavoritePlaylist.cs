using ChordKTV.Models.UserData;
using System.ComponentModel.DataAnnotations;

namespace ChordKTV.Models.Playlist;

public class UserFavoritePlaylist
{
    [Key]
    public Guid Id { get; set; } = Guid.NewGuid();
    public required Guid UserId { get; set; }
    public required string PlaylistUrl { get; set; }
    public DateTime FavoritedAt { get; set; }
} 