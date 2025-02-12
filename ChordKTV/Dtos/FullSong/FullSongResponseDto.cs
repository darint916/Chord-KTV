namespace ChordKTV.Dtos.FullSong;

public record FullSongResponseDto
(
    Guid Id,
    string Title,
    List<string> AlternateTitles,
    string Artist,
    List<string> FeaturedArtists,
    List<string> AlbumNames,
    DateOnly? ReleaseDate,
    string? Genre,
    string PlainLyrics,
    string LrcLyrics,
    string LrcRomanizedLyrics,
    string LrcTranslatedLyrics,
    string YouTubeUrl,
    List<string> AlternateYoutubeUrls,
    GeniusMetaDataDto GeniusMetaData
);
