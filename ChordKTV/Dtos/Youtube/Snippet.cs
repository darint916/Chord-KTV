namespace ChordKTV.Dtos.YouTubeApi;

public record Snippet(
    DateTime PublishedAt,
    string ChannelId,
    string Title,
    string Description,
    Thumbnails Thumbnails,
    string ChannelTitle,
    string LiveBroadcastContent
);
