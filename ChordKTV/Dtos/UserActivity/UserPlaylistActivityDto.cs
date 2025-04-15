namespace ChordKTV.Dtos.UserActivity;

public class UserPlaylistActivityDto
{
    public required string PlaylistUrl { get; set; }
    public DateTime? DatePlayed { get; set; }
}
