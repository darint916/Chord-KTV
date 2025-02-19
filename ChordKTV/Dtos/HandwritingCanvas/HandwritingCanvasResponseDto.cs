namespace ChordKTV.Dtos;

public record HandwritingCanvasResponseDto(
    string RecognizedText,
    double MatchPercentage
);
