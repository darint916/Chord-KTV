namespace ChordKTV.Dtos;

public record LrcLyricsDto(
    int LrcLibId,
    string Name, // LRCLIB returns both a name and a track name, storing both
    string TrackName,
    string ArtistName,
    string AlbumName,
    TimeSpan Duration,
    bool Instrumental,
    string PlainLyrics,
    string SyncedLyrics,
    string? RomanizedPlainLyrics,
    string? RomanizedSyncedLyrics
);
