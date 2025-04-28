using System.ComponentModel.DataAnnotations;

namespace ChordKTV.Models.SongData;

public class UserSongActivity
{
    [Key]
    public Guid Id { get; set; } = Guid.NewGuid();
    public required Song Song { get; set; }
    public Guid UserId { get; set; }
    public List<DateTime> DatesPlayed { get; set; } = new List<DateTime>();
    public DateTime LastPlayed { get; set; }
    public bool IsFavorite { get; set; }
    public DateTime? DateFavorited { get; set; }
}
