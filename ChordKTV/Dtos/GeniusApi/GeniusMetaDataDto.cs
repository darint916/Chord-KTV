namespace ChordKTV.Dtos.GeniusApi;

public record GeniusMetaDataDto(
    int GeniusId,
    string? HeaderImageUrl,
    string? SongImageUrl,
    LanguageCode Language
);
