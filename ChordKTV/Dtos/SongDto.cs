namespace ChordKTV.Dtos;

public record SongDto(
    string Name,
    string PrimaryArtist,
    List<string> FeaturedArtists,
    List<string> AlbumNames,
    string? Genre,
    string PlainLyrics,
    GeniusMetaDataDto GeniusMetaData
);
