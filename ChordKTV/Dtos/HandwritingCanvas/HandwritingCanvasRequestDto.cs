namespace ChordKTV.Dtos.HandwritingCanvas;

public record HandwritingCanvasRequestDto(
    string Image,
    LanguageCode Language,
    string ExpectedMatch
);
