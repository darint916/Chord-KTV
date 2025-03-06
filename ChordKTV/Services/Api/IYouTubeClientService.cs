using ChordKTV.Dtos;
using ChordKTV.Dtos.YouTubeApi;

namespace ChordKTV.Services.Api;

public interface IYouTubeClientService
{
    public Task<PlaylistDetailsDto?> GetPlaylistDetailsAsync(string playlistId, bool shuffle);
    public Task<Dictionary<string, VideoDetails>> GetVideosDetailsAsync(List<string> videoIds);
    public Task<string?> SearchYoutubeVideoLinkAsync(string title, string artist, string? album, TimeSpan? duration);
}
