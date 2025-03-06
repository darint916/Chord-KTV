namespace ChordKTV.Models.SongData;

using System.ComponentModel.DataAnnotations;

public class Song
{
    [Key]
    public Guid Id { get; set; } = Guid.NewGuid();

    [Required]
    public string Title { get; set; } = string.Empty;
    public List<string> AlternateTitles { get; set; } = [];
    public string Artist { get; set; } = string.Empty;
    public List<string> FeaturedArtists { get; set; } = [];
    public List<Album> Albums { get; set; } = [];
    public DateOnly? ReleaseDate { get; set; }
    public string? Genre { get; set; } //Optional for now? filled by community?
    public TimeSpan? Duration { get; set; }  //in seconds
    public string? PlainLyrics { get; set; } //from lyrics genius as backup
    public string? LrcLyrics { get; set; } //from RLC Library
    public string? LrcRomanizedLyrics { get; set; } //from LRC Library
    public string? LrcTranslatedLyrics { get; set; } //from LLM
    public int? LrcId { get; set; }
    public int? RomLrcId { get; set; }
    public string? YoutubeId { get; set; }
    public List<string> AlternateYoutubeIds { get; set; } = [];
    public GeniusMetaData GeniusMetaData { get; set; } = new GeniusMetaData();
}
