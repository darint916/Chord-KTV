namespace ChordKTV.Dtos;

using ChordKTV.Dtos.GeniusApi;

public record SongDto(
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
    string? YoutubeUrl,
    List<string> AlternateYoutubeUrls,
    GeniusMetaDataDto GeniusMetaData
);
