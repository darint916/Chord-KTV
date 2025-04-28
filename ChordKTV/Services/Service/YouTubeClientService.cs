using Google.Apis.Services;
using Google.Apis.YouTube.v3;
using Google.Apis.YouTube.v3.Data;
using ChordKTV.Services.Api;
using ChordKTV.Dtos;
using ChordKTV.Utils;
using ChordKTV.Dtos.YouTubeApi;
using ChordKTV.Data.Api.SongData;
using ChordKTV.Models.SongData;

namespace ChordKTV.Services.Service;

public class YouTubeApiClientService : IYouTubeClientService, IDisposable
{
    private readonly string? _youtubeApiKey;
    private readonly string? _youtubeSearchApiKey;
    private readonly ILogger<YouTubeApiClientService> _logger;
    private readonly YouTubeService _youTubeService;
    private readonly YouTubeService _youTubeSearchService;
    private readonly IYoutubeSongRepo _youtubeSongRepo;
    private readonly ISongRepo _songRepo;
    private bool _disposed;
    public YouTubeApiClientService(IConfiguration configuration, ILogger<YouTubeApiClientService> logger, ISongRepo songRepo, IYoutubeSongRepo youtubeSongRepo)
    {
        _logger = logger;
        _songRepo = songRepo;
        _youtubeSongRepo = youtubeSongRepo;
        _youtubeApiKey = configuration["YouTube:ApiKey"];
        if (string.IsNullOrEmpty(_youtubeApiKey))
        {
            _logger.LogError("YouTube API key not found in backend configuration");
            throw new ArgumentNullException(nameof(configuration), "YouTube:ApiKey is missing or not set in configuration.");
        }
        else
        {
            _youTubeService = new YouTubeService(
                new BaseClientService.Initializer
                {
                    ApiKey = _youtubeApiKey,
                    ApplicationName = "ChordKTV Base Key"
                }
            );
        }

        _youtubeSearchApiKey = configuration["YouTube:SearchApiKey"]; //separate as search is expensive
        if (string.IsNullOrEmpty(_youtubeSearchApiKey))
        {
            _logger.LogError("YouTube Search API key not found in backend configuration defaulting to normal API key");
            _youTubeSearchService = new YouTubeService(
                new BaseClientService.Initializer
                {
                    ApiKey = _youtubeApiKey,
                    ApplicationName = "ChordKTV Backup Search"
                }
            );
        }
        else
        {
            _youTubeSearchService = new YouTubeService(
                new BaseClientService.Initializer
                {
                    ApiKey = _youtubeSearchApiKey,
                    ApplicationName = "ChordKTV Search"
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

        string playlistTitle = "Unknown Playlist";
        string? playlistThumbnailUrl = null;
        if (playlistResponse.Items.Count > 0)
        {
            playlistTitle = playlistResponse.Items[0].Snippet.Title;
            playlistThumbnailUrl = playlistResponse.Items[0].Snippet.Thumbnails.Default__.ToString();
        }

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
            .Select(idBatch => GetVideosDetailsAsync(idBatch.ToList()));

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

        return new PlaylistDetailsDto(playlistTitle, videos, playlistThumbnailUrl);
    }

    public async Task<Dictionary<string, VideoDetails>> GetVideosDetailsAsync(List<string> videoIds)
    {
        var result = new Dictionary<string, VideoDetails>();

        // YouTube API allows up to 50 video IDs per request
        foreach (string[] idBatch in videoIds.Chunk(50))
        {
            VideosResource.ListRequest videoRequest = _youTubeService.Videos.List("snippet,contentDetails");
            videoRequest.Id = string.Join(",", idBatch);

            VideoListResponse videoResponse = await videoRequest.ExecuteAsync();

            foreach (Video video in videoResponse.Items)
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
                    video.Snippet.Title,
                    video.Snippet.ChannelTitle,
                    duration
                );
            }
        }
        return result;
    }

    public async Task<string?> SearchYoutubeVideoLinkAsync(string title, string artist, string? album, TimeSpan? duration, double durationTolerance = 3.5)
    {
        if (string.IsNullOrEmpty(_youtubeSearchApiKey))
        {
            return null;
        }
        //reference https://developers.google.com/youtube/v3/docs/search/list#.net

        SearchResource.ListRequest searchRequest = _youTubeSearchService.Search.List("snippet");
        searchRequest.Q = $"{title} {artist}"; //no album for now, as youtube search api is kinda lobotomized, will return no result
        searchRequest.Type = "video";
        searchRequest.MaxResults = 6; //more simple, maybe expand in future to allow users to choose, 2 groups based on relevancy sort
        searchRequest.VideoEmbeddable = SearchResource.ListRequest.VideoEmbeddableEnum.True__;

        //https://stackoverflow.com/a/17738994/17621099 category type 10 is music for all regions where allowed
        // searchRequest.VideoCategoryId = "10"; //disabled for now, might be too specific

        SearchListResponse searchResponse = await searchRequest.ExecuteAsync();
        //first search response item that has video id

        List<string> videoIds = searchResponse.Items
            .Where(item => item.Id.Kind == "youtube#video")
            .Select(item => item.Id.VideoId)
            .ToList();

        Dictionary<string, VideoDetails> videoDetailsDict = await GetVideosDetailsAsync(videoIds);

        if (duration.HasValue)
        {
            durationTolerance = Math.Abs(durationTolerance);
            string? withinDurationId = videoIds.FirstOrDefault(id =>
                videoDetailsDict.TryGetValue(id, out VideoDetails? details) &&
                Math.Abs((details.Duration - duration.Value).TotalSeconds) <= durationTolerance);

            if (withinDurationId != null)
            {
                return withinDurationId;
            }

            //return closest duration match
            return videoIds.MinBy(id => videoDetailsDict.TryGetValue(id, out VideoDetails? details)
                ? Math.Abs((details.Duration - duration.Value).TotalSeconds)
                : double.MaxValue);
        }
        return videoIds.FirstOrDefault();
    }

    public async Task<string?> PutYoutubeInstrumentalIdFromSongIdAsync(Guid songId)
    {
        if (string.IsNullOrEmpty(_youtubeSearchApiKey))
        {
            throw new InvalidOperationException("YouTube Search API key is not set in youtubeservice.");
        }

        Song? song = await _songRepo.GetSongByIdAsync(songId) ?? throw new KeyNotFoundException($"Song with ID {songId} not found.");
        if (!string.IsNullOrWhiteSpace(song.YoutubeInstrumentalId))
        {
            return song.YoutubeInstrumentalId;
        }
        //reference https://developers.google.com/youtube/v3/docs/search/list#.net
        SearchResource.ListRequest searchRequest = _youTubeSearchService.Search.List("snippet");
        searchRequest.Q = $"{song.Title} {song.Artist} instrumental"; //no album for now, as youtube search api is kinda lobotomized, will return no result
        searchRequest.Type = "video";
        searchRequest.MaxResults = 3; //more simple, maybe expand in future to allow users to choose, 2 groups based on relevancy sort
        searchRequest.VideoEmbeddable = SearchResource.ListRequest.VideoEmbeddableEnum.True__;

        SearchListResponse searchResponse = await searchRequest.ExecuteAsync();

        //first search response item that has video id
        List<string> videoIds = searchResponse.Items
            .Where(item => item.Id.Kind == "youtube#video")
            .Select(item => item.Id.VideoId)
            .ToList();

        Dictionary<string, VideoDetails> videoDetailsDict = await GetVideosDetailsAsync(videoIds);
        TimeSpan? duration = song.Duration;
        string? keymatch = null;

        //TOGGLE IF NEEDED, DISABLING DURATION FOR NOW UNTIL WE FIND ISSUE CASE WITH NON MATCHING DURATION

        // if (duration.HasValue)
        // {
        //     string? withinDurationId = videoIds.FirstOrDefault(id =>
        //         videoDetailsDict.TryGetValue(id, out VideoDetails? details) &&
        //         Math.Abs((details.Duration - duration.Value).TotalSeconds) <= 3.5);
        //     if (withinDurationId != null)
        //     {
        //         keymatch = withinDurationId;
        //     }
        //     else
        //     {
        //         keymatch = videoIds.MinBy(id => videoDetailsDict.TryGetValue(id, out VideoDetails? details)
        //             ? Math.Abs((details.Duration - duration.Value).TotalSeconds)
        //             : double.MaxValue);
        //     }
        // }
        // else
        // {
        keymatch = videoIds.FirstOrDefault();
        // }
        if (keymatch != null)
        {
            song.YoutubeInstrumentalId = keymatch;
            if (!song.AlternateTitles.Remove(keymatch))
            {
                await _youtubeSongRepo.AddYoutubeSongAsync(new YoutubeSong { YoutubeId = keymatch, Song = song });
            }
        }
        return keymatch;
    }

    //Below is the youtube service dispose stuff, needed as we abstracted the instances out, basically so they can be shared, these get handled by DI automatically
    protected virtual void Dispose(bool disposing)
    {
        if (!_disposed)
        {
            if (disposing)
            {
                // Dispose managed resources.
                _youTubeService?.Dispose();
                _youTubeSearchService?.Dispose();
            }
            _disposed = true;
        }
    }

    public void Dispose()
    {
        Dispose(disposing: true);
        GC.SuppressFinalize(this); // no need for finalizer if Dispose was called
    }

    ~YouTubeApiClientService() // finalizer (safeguard)
    {
        Dispose(disposing: false);
    }
}
