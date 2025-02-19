using ChordKTV.Dtos;

namespace ChordKTV.Services.Api;

public interface IHandwritingService
{
    public Task<bool?> RecognizeTextAsync(HandwritingCanvasDto image);
}