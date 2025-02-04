namespace ChordKTV.Services.Api;
public interface ILrcService
{
    public Task<string?> GetLrcLibLyricsAsync(string title, string artist, string? albumName, float? duration);
}
