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
        var allVideoIds = new List<string>();
        var playlistItems = new List<PlaylistItem>();

        // First gather all playlist items
        do
        {
            PlaylistItemListResponse playlistItemsResponse = await playlistItemsRequest.ExecuteAsync();
            playlistItems.AddRange(playlistItemsResponse.Items);
            allVideoIds.AddRange(playlistItemsResponse.Items.Select(item => item.ContentDetails.VideoId));
            playlistItemsRequest.PageToken = playlistItemsResponse.NextPageToken;
        }
        while (!string.IsNullOrEmpty(playlistItemsRequest.PageToken));

        // Batch video details requests in parallel
        IEnumerable<Task<Dictionary<string, VideoDetails>>> videoDetailsTasks = allVideoIds
            .Chunk(50)
            .Select(idBatch => GetVideosDetailsAsync(googleYouTube, idBatch.ToList()));

        Dictionary<string, VideoDetails>[] videoDetailsResults = await Task.WhenAll(videoDetailsTasks);
        var allVideoDetails = videoDetailsResults.SelectMany(dict => dict).ToDictionary(kvp => kvp.Key, kvp => kvp.Value);

        // Map playlist items to VideoInfo objects
        foreach (PlaylistItem item in playlistItems)
        {
            string videoId = item.ContentDetails.VideoId;
            if (allVideoDetails.TryGetValue(videoId, out VideoDetails? details))
            {
                videos.Add(new VideoInfo(
                    item.Snippet.Title,
                    details.ChannelTitle,
                    $"https://www.youtube.com/watch?v={videoId}",
                    details.Duration
                ));
            }
        }

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
