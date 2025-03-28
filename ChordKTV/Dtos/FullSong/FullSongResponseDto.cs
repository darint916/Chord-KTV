using ChordKTV.Dtos.GeniusApi;

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
    TimeSpan? Duration,
    string? Genre,
    string PlainLyrics,
    string LrcLyrics,
    string LrcRomanizedLyrics,
    string LrcTranslatedLyrics,
    string YouTubeId,
    List<string> AlternateYoutubeIds,
    GeniusMetaDataDto GeniusMetaData
);
