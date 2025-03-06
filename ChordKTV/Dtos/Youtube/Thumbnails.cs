namespace ChordKTV.Dtos.YouTubeApi;

public record Thumbnails(
    Thumbnail Default,
    Thumbnail Medium,
    Thumbnail High,
    Thumbnail? Standard, //better than high btw
    Thumbnail? Maxres
);
