using ChordKTV.Dtos.GeniusApi;

namespace ChordKTV.Dtos;

public record SongDto
(
    string Title,
    string Artist,
    List<string> FeaturedArtists,
    List<string> AlbumNames,
    string? Genre,
    string? PlainLyrics,
    string? LrcLyrics,
    string? LrcRomanizedLyrics,
    string? LrcTranslatedLyrics,
    string? YouTubeId,
    List<string> AlternateYoutubeIds,
    GeniusMetaDataDto GeniusMetaData
);
