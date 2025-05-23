using System.ComponentModel.DataAnnotations;

namespace ChordKTV.Models.SongData;

public class Album
{
    //TODO: Make name + artist required
    [Key]
    public Guid Id { get; set; } = Guid.NewGuid();
    public bool IsSingle { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Artist { get; set; } = string.Empty;
    public List<Song> Songs { get; set; } = [];
}
