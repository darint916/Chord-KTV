using System.ComponentModel.DataAnnotations;
using ChordKTV.Dtos;

namespace ChordKTV.Models.SongData;

public class GeniusMetaData
{
    [Key]
    public int GeniusId { get; set; } = 0;
    public string? HeaderImageUrl { get; set; }
    public string? HeaderImageThumbnailUrl { get; set; }
    public string? SongImageUrl { get; set; }
    public string? SongImageThumbnailUrl { get; set; }
    public LanguageCode Language { get; set; } = LanguageCode.UNK;
}
