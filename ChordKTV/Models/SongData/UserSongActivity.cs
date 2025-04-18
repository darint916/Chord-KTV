using System.ComponentModel.DataAnnotations;
using ChordKTV.Models.UserData;

namespace ChordKTV.Models.SongData;

public class UserSongActivity
{
    [Key]
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid SongId { get; set; }
    public Song Song { get; set; } = null!;
    public Guid UserId { get; set; }
    public List<DateTime> PlayDates { get; set; } = new List<DateTime>();
    public bool IsFavorite { get; set; } = false;
}
