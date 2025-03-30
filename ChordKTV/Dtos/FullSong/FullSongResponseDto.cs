using ChordKTV.Dtos.GeniusApi;

namespace ChordKTV.Dtos.FullSong;

public class FullSongResponseDto
{
    public required Guid Id { get; init; }
    public required string Title { get; init; }
    public required List<string> AlternateTitles { get; init; }
    public required string Artist { get; init; }
    public required List<string> FeaturedArtists { get; init; }
    public required List<string> AlbumNames { get; init; }
    public DateOnly? ReleaseDate { get; init; }
    public TimeSpan? Duration { get; init; }
    public string? Genre { get; init; }
    public required string PlainLyrics { get; init; }
    public required string LrcLyrics { get; init; }
    public required string LrcRomanizedLyrics { get; init; }
    public required string LrcTranslatedLyrics { get; init; }
    public string? YouTubeId { get; init; }
    public required List<string> AlternateYoutubeIds { get; init; }
    public required GeniusMetaDataDto GeniusMetaData { get; init; }

    // Mutable match score fields
    public MatchScores TitleMatchScores { get; set; } = new();
    public MatchScores ArtistMatchScores { get; set; } = new();
}
