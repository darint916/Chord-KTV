using Google.Apis.Services;
using Google.Apis.YouTube.v3;
using Google.Apis.YouTube.v3.Data;
using ChordKTV.Services.Api;
using ChordKTV.Dtos;
using ChordKTV.Utils;

namespace ChordKTV.Services.Service;

public class YouTubeApiClientService : IYouTubeClientService
{
    private readonly string? _youtubeApiKey;
    private readonly string? _youtubeSearchApiKey;
    private readonly ILogger<YouTubeApiClientService> _logger;
    private readonly YouTubeService _youTubeService;
    private readonly YouTubeService _youTubeSearchService;
    public YouTubeApiClientService(IConfiguration configuration, ILogger<YouTubeApiClientService> logger)
    {
        _logger = logger;
        _youtubeApiKey = configuration["YouTube:ApiKey"];
        if (string.IsNullOrEmpty(_youtubeApiKey))
        {
            _logger.LogError("YouTube API key not found in backend configuration");
        }
        else
        {
            _youTubeService = new YouTubeService(
                new BaseClientService.Initializer
                {
                    ApiKey = _youtubeApiKey,
                    ApplicationName = "ChordKTV"
                }
            );
        }

        _youtubeSearchApiKey = configuration["YouTube:SearchApiKey"]; //separate as search is expensive
        if (string.IsNullOrEmpty(_youtubeSearchApiKey))
        {
            _logger.LogError("YouTube Search API key not found in backend configuration");
        }
        else
        {
            _youTubeSearchService = new YouTubeService(
                new BaseClientService.Initializer
                {
                    ApiKey = _youtubeSearchApiKey,
                    ApplicationName = "ChordKTV"
                }
            );
        }
    }

    public async Task<PlaylistDetailsDto?> GetPlaylistDetailsAsync(string playlistId, bool shuffle)
    {
        if (string.IsNullOrEmpty(_youtubeApiKey))
        {
            return null;
        }

        PlaylistsResource.ListRequest playlistRequest = _youTubeService.Playlists.List("snippet");
        playlistRequest.Id = playlistId;

        PlaylistListResponse playlistResponse = await playlistRequest.ExecuteAsync();

        string playlistTitle = (playlistResponse.Items.Count > 0)
            ? playlistResponse.Items[0].Snippet.Title
            : "Unknown Playlist";

        PlaylistItemsResource.ListRequest playlistItemsRequest = _youTubeService.PlaylistItems.List("snippet,contentDetails");
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

    public async Task<string?> GetYoutubeVideoLinkAsync(string title, string artist, string? album, TimeSpan? duration)
    {
        if (string.IsNullOrEmpty(_youtubeSearchApiKey))
        {
            return null;
        }
        //reference https://developers.google.com/youtube/v3/docs/search/list#.net

        SearchResource.ListRequest searchRequest = _youTubeSearchService.Search.List("snippet");
        searchRequest.Q = $"{title} {artist} {album ?? ""}";
        searchRequest.Type = "video";
        searchRequest.MaxResults = 3; //less, and then we compare durations

        //https://stackoverflow.com/a/17738994/17621099 category type 10 is music for all regions where allowed
        searchRequest.VideoCategoryId = "10";

        SearchListResponse searchResponse = await searchRequest.ExecuteAsync();
        if (duration.HasValue)
        {
            //order by duration
            
        }

    }
}
