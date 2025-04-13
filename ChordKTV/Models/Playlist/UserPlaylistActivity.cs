using System.ComponentModel.DataAnnotations;
using ChordKTV.Models.UserData;

namespace ChordKTV.Models.Playlist;

public class UserPlaylistActivity
{
    [Key]
    public Guid Id { get; set; } = Guid.NewGuid();
    public required string PlaylistUrl { get; set; }
    public int PlayCount { get; set; }
    public DateTime LastPlayed { get; set; }
    public Guid UserId { get; set; }
} 