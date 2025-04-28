namespace ChordKTV.Dtos.UserActivity;
using System;

public class UserSongActivityFavoriteRequestDto
{
    public Guid SongId { get; set; }
    public bool IsFavorite { get; set; }
}
