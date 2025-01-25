using System.ComponentModel.DataAnnotations;

namespace ChordKTV.Models.SongData;

public class GeniusMetaData
{
    [Key]
    public int GeniusId { get; set; } = 0;
    public string? HeaderImageUrl { get; set; } = null;
    public string? HeaderImageThumbnailUrl { get; set; } = null;
    public string? SongImageUrl { get; set; } = null;
    public string? SongImageThumbnailUrl { get; set; } = null;
}
