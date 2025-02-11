namespace ChordKTV.Dtos.FullSong;

public record FullSongResponseDto
(
    Guid Id,
    string Title,
    string Artist,
    string AlbumName,
    string Lyrics,
    string YouTubeUrl,
    string? LrcLyrics,
    string? LrcRomanizedLyrics
);
