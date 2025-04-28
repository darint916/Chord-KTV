namespace ChordKTV.Dtos.UserActivity;

public class UserPlaylistActivityFavoriteRequestDto
{
    public string PlaylistId { get; set; } = string.Empty;
    public bool IsFavorite { get; set; }
}
