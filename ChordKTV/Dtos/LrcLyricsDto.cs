using System;

namespace ChordKTV.Dtos;

public class LrcLyricsDto
{
    public int Id { get; set; } // ID of original language song in LRCLIB
    public int? RomanizedId { get; set; } // ID of romanized song in LRCLIB
    public string? Name { get; set; } // LRCLIB returns both a name and a track name, storing both
    public string? TrackName { get; set; }
    public string? ArtistName { get; set; }
    public string? AlbumName { get; set; }
    public float? Duration { get; set; }
    public bool Instrumental { get; set; }
    public string? PlainLyrics { get; set; }
    public string? SyncedLyrics { get; set; }
    public string? RomanizedPlainLyrics { get; set; }
    public string? RomanizedSyncedLyrics { get; set; }
    public HashSet<string> AlternateTitles { get; set; } = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
    public HashSet<string> AlternateArtists { get; set; } = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
    public MatchScores TitleMatchScores { get; set; } = new();
    public MatchScores ArtistMatchScores { get; set; } = new();
}
