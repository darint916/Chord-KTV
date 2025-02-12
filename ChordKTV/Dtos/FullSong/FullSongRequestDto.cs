namespace ChordKTV.Dtos.FullSong;

public record FullSongRequestDto
(
    string? Title,
    string? Artist,
    TimeSpan? Duration,
    string? Lyrics,
    string? YouTubeUrl
);
