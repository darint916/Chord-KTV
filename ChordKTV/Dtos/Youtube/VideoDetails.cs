namespace ChordKTV.Dtos.YouTubeApi;
public record VideoDetails(
    string Title,
    string ChannelTitle,
    TimeSpan Duration
);
