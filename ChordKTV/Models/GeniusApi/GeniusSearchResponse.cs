namespace ChordKTV.Models.GeniusApi;

public class GeniusSearchResponse
{
    public GeniusMeta Meta { get; set; } = new();
    public GeniusSearchResponseData Response { get; set; } = new();
}

public class GeniusMeta
{
    public int Status { get; set; }
}

public class GeniusSearchResponseData
{
    public List<GeniusHit> Hits { get; set; } = [];
}

public class GeniusHit
{
    public GeniusResult Result { get; set; } = new();
}

public class GeniusResult
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string HeaderImageUrl { get; set; } = string.Empty;
    public string SongArtImageUrl { get; set; } = string.Empty;
    public string PrimaryArtistNames { get; set; } = string.Empty;
    public GeniusAlbum? Album { get; set; }
}

public class GeniusAlbum
{
    public string Name { get; set; } = string.Empty;
} 