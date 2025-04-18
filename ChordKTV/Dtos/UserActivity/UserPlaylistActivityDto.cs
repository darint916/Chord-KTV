namespace ChordKTV.Dtos.UserActivity;
using System;
using System.Collections.Generic;

public class UserPlaylistActivityDto
{
    public string PlaylistUrl { get; set; } = string.Empty;
    public bool IsFavorite { get; set; }
    public List<DateTime> DatesPlayed { get; set; } = new List<DateTime>();
}
