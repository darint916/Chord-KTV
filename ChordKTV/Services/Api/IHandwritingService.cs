using ChordKTV.Dtos.HandwritingCanvas;

namespace ChordKTV.Services.Api;

public interface IHandwritingService
{
    public Task<HandwritingCanvasResponseDto?> RecognizeTextAsync(HandwritingCanvasRequestDto image);
}
