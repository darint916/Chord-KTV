namespace ChordKTV.Models.SongData;

using System.ComponentModel.DataAnnotations;

public class GeniusMetaData
{
    [Key]
    public int GeniusId { get; set; } = 0;
    public string? HeaderImageUrl { get; set; }
    public string? HeaderImageThumbnailUrl { get; set; }
    public string? SongImageUrl { get; set; }
    public string? SongImageThumbnailUrl { get; set; }
}
