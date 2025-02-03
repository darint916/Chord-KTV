namespace ChordKTV.Services.Api;
public interface ILRCService
{
    public Task<string?> GetLRCLIBLyrics(string title, string artist, string? albumName, float? duration);
}
