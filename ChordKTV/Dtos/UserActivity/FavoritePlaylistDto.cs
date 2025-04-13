namespace ChordKTV.Dtos.UserActivity;

public record FavoritePlaylistDto(
    string PlaylistUrl,
    bool Favorited
);
