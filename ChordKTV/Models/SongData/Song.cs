namespace ChordKTV.Models.SongData;

using System.ComponentModel.DataAnnotations;

public class Song
{
    [Key]
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Title { get; set; } = string.Empty;
    public List<string> AlternateTitles { get; set; } = [];
    public string Artist { get; set; } = string.Empty;
    public List<string> FeaturedArtists { get; set; } = [];
    public List<Album> Albums { get; set; } = [];
    public DateOnly? ReleaseDate { get; set; }
    public string? Genre { get; set; } = string.Empty; //Optional for now? filled by community?
    public string LrcLyrics { get; set; } = string.Empty; //from RLC Library
    public TimeSpan? SongDuration { get; set; }  //in seconds
    public string PlainLyrics { get; set; } = string.Empty; //from lyrics genius as backup
    public string RomanizedLyrics { get; set; } = string.Empty; //from LRC Library
    public string LLMTranslation { get; set; } = string.Empty; //from LLM
    public GeniusMetaData GeniusMetaData { get; set; } = new GeniusMetaData();
    public int LrcId { get; set; }
    public int RomLrcId { get; set; }
    public string YoutubeUrl { get; set; } = string.Empty;
    public List<string> AlternateYoutubeUrls { get; set; } = [];
}
