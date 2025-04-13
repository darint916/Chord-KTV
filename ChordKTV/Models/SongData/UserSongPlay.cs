using System.ComponentModel.DataAnnotations;
using ChordKTV.Models.UserData;

namespace ChordKTV.Models.SongData;

public class UserSongPlay
{
    [Key]
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid SongId { get; set; }
    public Song Song { get; set; } = null!;
    public DateTime PlayedAt { get; set; }
    public Guid UserId { get; set; }
}
