using ChordKTV.Dtos;

namespace ChordKTV.Services.Api;

public interface IHandwritingService
{
    public Task<HandwritingCanvasResponseDto?> RecognizeTextAsync(HandwritingCanvasRequestDto image);
}
