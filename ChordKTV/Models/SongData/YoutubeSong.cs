using System.ComponentModel.DataAnnotations;

namespace ChordKTV.Models.SongData;

public class YoutubeSong
{
    [Key]
    public string YoutubeId { get; set; } = string.Empty; //should be populated when adding
    [Required]
    public required Song Song { get; set; }
}
