using System.ComponentModel.DataAnnotations;

namespace ChordKTV.Models.Playlist;

public class UserPlaylistActivity
{
    [Key]
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid UserId { get; set; }
    public required string PlaylistId { get; set; }
    public required string PlaylistThumbnailUrl { get; set; }
    public List<DateTime> DatesPlayed { get; set; } = new List<DateTime>();
    public DateTime LastPlayed { get; set; }
    public bool IsFavorite { get; set; }
    public DateTime? DateFavorited { get; set; }
}
