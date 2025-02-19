namespace ChordKTV.Dtos;

public record HandwritingCanvasRequestDto(
    string Image,
    LanguageCode Language,
    string ExpectedMatch
);
