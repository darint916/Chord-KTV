namespace ChordKTV.Models.SongData;

using System.ComponentModel.DataAnnotations;

public class Album
{
    [Key]
    public Guid Id { get; set; } = Guid.NewGuid();
    public bool IsSingle { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Artist { get; set; } = string.Empty;
    public List<Song> Songs { get; set; } = [];
}
