namespace ChordKTV.Dtos.YouTubeApi;

public record ResourceId(
    string Kind, // "youtube#video" or "youtube#channel" or "youtube#playlist" which dictates below which is not null
    string? VideoId,
    string? ChannelId,
    string? PlaylistId
);
