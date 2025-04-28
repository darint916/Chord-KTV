namespace ChordKTV.Dtos.UserActivity;

public class UserPlaylistActivityFavoriteRequestDto
{
    public string PlaylistId { get; set; } = string.Empty;
    public bool IsFavorite { get; set; }
    public bool IsPlayed { get; set; }
}
