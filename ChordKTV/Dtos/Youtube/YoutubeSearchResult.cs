namespace ChordKTV.Dtos.YouTubeApi;

public record YoutubeSearchResult(
    string Kind,
    string Etag,
    ResourceId Id,
    Snippet Snippet,
    string ChannelTitle,
    string LiveBroadcastContent
);
