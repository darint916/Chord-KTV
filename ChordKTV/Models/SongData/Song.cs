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
    public string Genre { get; set; } = string.Empty; //Optional for now? filled by community?
    public string SyncLyrics { get; set; } = string.Empty; //from RLC Library
    public int? SongDuration { get; set; } = null; //in seconds
    public bool IsInstrumental { get; set; } = false;
    public string PlainLyrics { get; set; } = string.Empty; //from lyrics genius as backup
    public string LLMTranslation { get; set; } = string.Empty; //from LLM
    public string? HeaderImageUrl { get; set; } = null;
    public string? HeaderImageThumbnailUrl { get; set; } = null;
    public string? SongImageUrl { get; set; } = null;
    public string? SongImageThumbnailUrl { get; set; } = null;
    public bool? InGenius { get; set; } = null;
    public int GeniusId { get; set; } = 0;
    public bool? InLRCLib { get; set; } = null;
    public int LrcId { get; set; } = 0;
    public bool? InSpotify { get; set; } = null;
    public bool? InYoutube { get; set; } = null;
    public string YoutubeUrl { get; set; } = string.Empty;
    public List<string> AlternateYoutubeUrls { get; set; } = new List<string>();
}