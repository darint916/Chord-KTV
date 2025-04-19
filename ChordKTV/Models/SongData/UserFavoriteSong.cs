using System;
using System.ComponentModel.DataAnnotations;
using ChordKTV.Models.UserData;
using ChordKTV.Models.SongData;

namespace ChordKTV.Models.SongData;

public class UserFavoriteSong
{
    [Key]
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid SongId { get; set; }
    public DateTime DateFavorited { get; set; }
    public Guid UserId { get; set; }
}
