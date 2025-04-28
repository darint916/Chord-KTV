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
    public string Title { get; set; } = string.Empty;
    public string Artist { get; set; } = string.Empty;
    public string? GeniusThumbnailUrl { get; set; } = string.Empty;
    public string? YoutubeThumbnailUrl { get; set; } = string.Empty;
}
