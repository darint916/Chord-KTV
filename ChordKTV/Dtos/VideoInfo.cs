namespace ChordKTV.Dtos;

public record VideoInfo
(
    string Title,
    string Artist,
    string Url,
    TimeSpan Duration
);
