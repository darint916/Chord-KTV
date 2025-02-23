namespace ChordKTV.Dtos.FullSong;

public record FullSongRequestDto
(
    string? Title,
    string? Artist,
    string? Album,
    TimeSpan? Duration,
    string? Lyrics,
    string? YouTubeUrl
);
