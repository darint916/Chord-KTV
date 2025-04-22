namespace ChordKTV.Dtos.UserActivity;
using System;
using System.Collections.Generic;

public class UserSongActivityDto
{
    public Guid SongId { get; set; }
    public bool IsFavorite { get; set; }
    public List<DateTime> DatesPlayed { get; set; } = new List<DateTime>();
    public DateTime LastPlayed { get; set; }
    public DateTime? DateFavorited { get; set; }
}
