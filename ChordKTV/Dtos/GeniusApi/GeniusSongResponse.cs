namespace ChordKTV.Dtos.GeniusApi;

public class GeniusSongResponse
{
    public GeniusMeta Meta { get; set; } = new();
    public GeniusSongResponseData Response { get; set; } = new();
}

public class GeniusSongResponseData
{
    public GeniusSongDetails Song { get; set; } = new();
}

public class GeniusSongDetails
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Language { get; set; } = string.Empty;
    public List<GeniusArtist> FeaturedArtists { get; set; } = [];
    public GeniusAlbum? Album { get; set; }
    public string ReleaseDate { get; set; } = string.Empty;
}

public class GeniusArtist
{
    public string Name { get; set; } = string.Empty;
}
