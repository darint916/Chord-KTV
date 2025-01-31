namespace ChordKTV.Services.Api
{
    public interface IYouTubeService
    {
        Task<string> GetPlaylistDetails(string playlistId);
    }
}
