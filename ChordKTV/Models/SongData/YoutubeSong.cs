using System.ComponentModel.DataAnnotations;

namespace ChordKTV.Models.SongData;

public class YoutubeSong
{
    [Key]
    public string YoutubeId { get; set; } = string.Empty;
    [Required]
    public required Song Song { get; set; }
}
