using ChordKTV.Dtos;
using ChordKTV.Dtos.YouTubeApi;

namespace ChordKTV.Services.Api;

public interface IYouTubeClientService
{
    public Task<PlaylistDetailsDto?> GetPlaylistDetailsAsync(string playlistId, bool shuffle, bool noVideoDetails = false);
    public Task<Dictionary<string, VideoDetails>> GetVideosDetailsAsync(List<string> videoIds);
    public Task<string?> SearchYoutubeVideoLinkAsync(string title, string artist, string? album, TimeSpan? duration, double durationTolerance = 3.5);
    public Task<string?> PutYoutubeInstrumentalIdFromSongIdAsync(Guid songId);
}
