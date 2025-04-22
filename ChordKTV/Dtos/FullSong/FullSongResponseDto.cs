using ChordKTV.Dtos.GeniusApi;
using ChordKTV.Dtos.LrcLib;

namespace ChordKTV.Dtos.FullSong;

public class FullSongResponseDto
{
    public Guid Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public List<string> AlternateTitles { get; set; } = new();
    public string Artist { get; set; } = string.Empty;
    public List<string> FeaturedArtists { get; set; } = new();
    public List<string> AlbumNames { get; set; } = new();
    public DateOnly? ReleaseDate { get; set; }
    public TimeSpan? Duration { get; set; }
    public string? Genre { get; set; }
    public string PlainLyrics { get; set; } = string.Empty;
    public string LrcLyrics { get; set; } = string.Empty;
    public string LrcRomanizedLyrics { get; set; } = string.Empty;
    public string LrcTranslatedLyrics { get; set; } = string.Empty;
    public string? YouTubeId { get; set; }
    public List<string> AlternateYoutubeIds { get; set; } = new();
    public GeniusMetaDataDto GeniusMetaData { get; set; } = new(0, null, null, LanguageCode.UNK);
    public MatchScores TitleMatchScores { get; set; } = new();
    public MatchScores ArtistMatchScores { get; set; } = new();
}
