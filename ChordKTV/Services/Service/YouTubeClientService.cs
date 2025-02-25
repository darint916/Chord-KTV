using Google.Apis.Services;
using Google.Apis.YouTube.v3;
using Google.Apis.YouTube.v3.Data;
using ChordKTV.Services.Api;
using ChordKTV.Dtos;
using ChordKTV.Utils;

namespace ChordKTV.Services.Service;

public class YouTubeApiClientService : IYouTubeClientService
{
    private readonly string? _apiKey;

    public YouTubeApiClientService(IConfiguration configuration)
    {
        _apiKey = configuration["YouTube:ApiKey"];
    }

    public async Task<PlaylistDetailsDto?> GetPlaylistDetailsAsync(string playlistId, bool shuffle)
    {
        if (string.IsNullOrEmpty(_apiKey))
        {
            return null;
        }

        var googleYouTube = new YouTubeService(
            new BaseClientService.Initializer
            {
                ApiKey = _apiKey,
                ApplicationName = "ChordKTV"
            }
        );

        PlaylistsResource.ListRequest playlistRequest = googleYouTube.Playlists.List("snippet");
        playlistRequest.Id = playlistId;

        PlaylistListResponse playlistResponse = await playlistRequest.ExecuteAsync();

        string playlistTitle = (playlistResponse.Items.Count > 0)
            ? playlistResponse.Items[0].Snippet.Title
            : "Unknown Playlist";

        PlaylistItemsResource.ListRequest playlistItemsRequest = googleYouTube.PlaylistItems.List("snippet,contentDetails");
        playlistItemsRequest.PlaylistId = playlistId;
        playlistItemsRequest.MaxResults = 50;

        var videos = new List<VideoInfo>();
        PlaylistItemListResponse playlistItemsResponse;
        do
        {
            playlistItemsResponse = await playlistItemsRequest.ExecuteAsync();

            // Get all video IDs from the current page
            var videoIds = playlistItemsResponse.Items.Select(item => item.ContentDetails.VideoId).ToList();

            // Get video details including duration and correct channel info
            Dictionary<string, VideoDetails> videoDetails = await GetVideosDetailsAsync(googleYouTube, videoIds);

            foreach (PlaylistItem item in playlistItemsResponse.Items)
            {
                string videoId = item.ContentDetails.VideoId;
                if (videoDetails.TryGetValue(videoId, out VideoDetails? details))
                {
                    string title = item.Snippet.Title;
                    string url = $"https://www.youtube.com/watch?v={videoId}";

                    videos.Add(new VideoInfo(
                        title,
                        details.ChannelTitle,  // Use the correct channel name
                        url,
                        details.Duration
                    ));
                }
            }

            playlistItemsRequest.PageToken = playlistItemsResponse.NextPageToken;
        }
        while (!string.IsNullOrEmpty(playlistItemsRequest.PageToken));

        if (shuffle)
        {
            Shuffle.FisherYatesShuffle(videos);
        }

        return new PlaylistDetailsDto(playlistTitle, videos);
    }

    private sealed record VideoDetails(string ChannelTitle, TimeSpan Duration);

    private static async Task<Dictionary<string, VideoDetails>> GetVideosDetailsAsync(YouTubeService youTubeService, List<string> videoIds)
    {
        var result = new Dictionary<string, VideoDetails>();

        // YouTube API allows up to 50 video IDs per request
        foreach (string[] idBatch in videoIds.Chunk(50))
        {
            VideosResource.ListRequest videoRequest = youTubeService.Videos.List("snippet,contentDetails");
            videoRequest.Id = string.Join(",", idBatch);

            VideoListResponse videoResponse = await videoRequest.ExecuteAsync();

            foreach (Video? video in videoResponse.Items)
            {
                TimeSpan duration = TimeSpan.Zero;
                try
                {
                    duration = System.Xml.XmlConvert.ToTimeSpan(video.ContentDetails.Duration);
                }
                catch
                {
                    // Keep default TimeSpan.Zero if parsing fails
                }

                result[video.Id] = new VideoDetails(
                    video.Snippet.ChannelTitle,
                    duration
                );
            }
        }
        return result;
    }

}
