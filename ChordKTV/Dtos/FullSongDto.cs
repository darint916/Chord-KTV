namespace ChordKTV.Dtos;

public class FullSongDto
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Name { get; set; } = string.Empty;
    public List<string> AlternateNames { get; set; } = [];
    public string PrimaryArtist { get; set; } = string.Empty;
    public List<string> FeaturedArtists { get; set; } = [];
    public DateOnly? ReleaseDate { get; set; }
    public string? Genre { get; set; } = string.Empty; //Optional for now? filled by community?
    public string SyncLyrics { get; set; } = string.Empty; //from RLC Library
    public TimeSpan? SongDuration { get; set; }  //in seconds
    public string PlainLyrics { get; set; } = string.Empty; //from lyrics genius as backup
    public string LLMTranslation { get; set; } = string.Empty; //from LLM
    public GeniusMetaDataDto? GeniusMetaData { get; set; }
    public int LrcId { get; set; }
    public string YoutubeUrl { get; set; } = string.Empty;
    public List<string> AlternateYoutubeUrls { get; set; } = [];
}
