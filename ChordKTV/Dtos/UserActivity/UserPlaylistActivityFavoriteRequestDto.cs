namespace ChordKTV.Dtos.UserActivity;

public class UserPlaylistActivityFavoriteRequestDto
{
    public string PlaylistUrl { get; set; } = string.Empty;
    public bool IsFavorite { get; set; }
}
