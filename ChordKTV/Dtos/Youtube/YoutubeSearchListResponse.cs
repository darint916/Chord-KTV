namespace ChordKTV.Dtos.YouTubeApi;

public record YoutubeSearchListResponse(
    string Kind,
    string Etag,
    string? NextPageToken,
    string? PrevPageToken,
    string RegionCode,
    PageInfo PageInfo,
    IEnumerable<YoutubeSearchResult> Items
);
