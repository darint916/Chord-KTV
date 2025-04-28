namespace ChordKTV.Dtos;

public record PlaylistDetailsDto
(
    string PlaylistTitle,
    List<VideoInfo> Videos,
    string? PlaylistThumbnailUrl
);
