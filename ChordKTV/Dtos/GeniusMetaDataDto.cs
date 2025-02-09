using ChordKTV.Dtos;

namespace ChordKTV.Dtos;

public record GeniusMetaDataDto(
    int GeniusId,
    string? HeaderImageUrl,
    string? SongImageUrl,
    LanguageCode Language
);
