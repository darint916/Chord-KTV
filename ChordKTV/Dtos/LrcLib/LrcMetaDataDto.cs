namespace ChordKTV.Dtos.LrcLib;

public class LrcMetaDataDto
{
    public int Id { get; set; }
    public required string Title { get; set; }
    public string? Artist { get; set; }
    public string? AlbumName { get; set; }
    public TimeSpan? Duration { get; set; }
}
