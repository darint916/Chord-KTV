namespace ChordKTV.Dtos.UserActivity;

public record FavoriteSongDto(
    Guid SongId,
    bool Favorited
); 