using System.ComponentModel.DataAnnotations;
using ChordKTV.Models.UserData;

namespace ChordKTV.Models.SongData;

public class UserSongActivity
{
    [Key]
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid SongId { get; set; }
    public Guid UserId { get; set; }
    public List<DateTime> DatesPlayed { get; set; } = new List<DateTime>();
    public DateTime LastPlayed { get; set; }
    public bool IsFavorite { get; set; }
    public DateTime? DateFavorited { get; set; }
}
