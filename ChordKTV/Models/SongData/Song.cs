using System.ComponentModel.DataAnnotations;

namespace ChordKTV.Models.SongData;

public class Song
{
    [Key]
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Name { get; set; } = string.Empty;
    public List<string> AlternateNames { get; set; } = new List<string>();
    public string PrimaryArtist { get; set; } = string.Empty;
    public List<string> FeaturedArtists { get; set; } = new List<string>();
    public List<Album> Albums { get; set; } = new List<Album>();
    public DateOnly? ReleaseDate { get; set; } = null;
    public string? Genre { get; set; } = string.Empty; //Optional for now? filled by community?
    public string SyncLyrics { get; set; } = string.Empty; //from RLC Library
    public TimeSpan? SongDuration { get; set; } = null; //in seconds
    public string PlainLyrics { get; set; } = string.Empty; //from lyrics genius as backup
    public string LLMTranslation { get; set; } = string.Empty; //from LLM
    public GeniusMetaData GeniusMetaData { get; set; } = new GeniusMetaData();
    public int LrcId { get; set; } = 0;
    public string YoutubeUrl { get; set; } = string.Empty;
    public List<string> AlternateYoutubeUrls { get; set; } = new List<string>();
}