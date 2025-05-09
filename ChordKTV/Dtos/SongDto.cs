using ChordKTV.Dtos.GeniusApi;

namespace ChordKTV.Dtos;

public record SongDto
(
    Guid Id,
    string Title,
    List<string> AlternateTitles,
    string Artist,
    List<string> FeaturedArtists,
    List<string> AlbumNames,
    DateOnly? ReleaseDate,
    string? Genre,
    TimeSpan? Duration,
    string? PlainLyrics,
    string? LrcLyrics,
    string? LrcRomanizedLyrics,
    string? LrcTranslatedLyrics,
    int? LrcId,
    int? RomLrcId,
    string? YouTubeId,
    string? YouTubeInstrumentalId,
    List<string> AlternateYoutubeIds,
    GeniusMetaDataDto GeniusMetaData
);
