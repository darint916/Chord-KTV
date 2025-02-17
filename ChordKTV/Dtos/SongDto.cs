namespace ChordKTV.Dtos;

using ChordKTV.Dtos.GeniusApi;

public record SongDto(
    string Title,
    string Artist,
    List<string> FeaturedArtists,
    List<string> AlbumNames,
    string? Genre,
    string? PlainLyrics,
    string? LrcLyrics,
    string? LrcRomanizedLyrics,
    string? LrcTranslatedLyrics,
    string? YouTubeUrl,
    List<string> AlternateYoutubeUrls,
    GeniusMetaDataDto GeniusMetaData
);
