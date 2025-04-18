using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using ChordKTV.Models.UserData;

namespace ChordKTV.Models.Playlist;

public class UserPlaylistActivity
{
    [Key]
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid UserId { get; set; }
    public required string PlaylistUrl { get; set; }
    public List<DateTime> PlayDates { get; set; } = new List<DateTime>();
    public bool IsFavorite { get; set; } = false;
}
