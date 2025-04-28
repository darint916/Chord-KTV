namespace ChordKTV.Dtos.UserActivity;
using System;
using System.Collections.Generic;

public class UserPlaylistActivityDto
{
    public string PlaylistId { get; set; } = string.Empty;
    public required string PlaylistThumbnailUrl { get; set; }
    public required string PlaylistTitle { get; set; }
    public List<DateTime> DatesPlayed { get; set; } = new List<DateTime>();
    public DateTime LastPlayed { get; set; }
    public bool IsFavorite { get; set; }
    public DateTime? DateFavorited { get; set; }
}
