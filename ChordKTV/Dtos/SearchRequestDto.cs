namespace ChordKTV.Dtos;

public record SearchRequestDto
(
    string? Title,
    string? Artist,
    TimeSpan? Duration,
    string? Lyrics
);
