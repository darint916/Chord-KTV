namespace ChordKTV.Dtos.HandwritingCanvas;

public record HandwritingCanvasResponseDto(
    string RecognizedText,
    double MatchPercentage
);
