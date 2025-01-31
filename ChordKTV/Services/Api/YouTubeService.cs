using System.Threading.Tasks;
using Google.Apis.Services;
using Google.Apis.YouTube.v3;
using Newtonsoft.Json;

namespace ChordKTV.Services.Api
{
    public class YouTubeApiClient : IYouTubeService
    {
        private readonly string apiKey = "AIzaSyBAPb2RMDrsEryFo5UnYXo6Du4nVopZwaA";

        public async Task<string> GetPlaylistDetails(string playlistId)
        {
            // Use fully qualified Google class name to avoid confusion:
            var googleYouTube = new Google.Apis.YouTube.v3.YouTubeService(
                new BaseClientService.Initializer
                {
                    ApiKey = apiKey,
                    ApplicationName = "ChordKTV"
                }
            );

            // Fetch playlist and video details as per previous logic
            var playlistRequest = googleYouTube.Playlists.List("snippet");
            playlistRequest.Id = playlistId;

            var playlistResponse = await playlistRequest.ExecuteAsync();

            string playlistTitle = playlistResponse.Items.Count > 0 
                ? playlistResponse.Items[0].Snippet.Title 
                : "Unknown Playlist";

            var playlistItemsRequest = googleYouTube.PlaylistItems.List("snippet,contentDetails");
            playlistItemsRequest.PlaylistId = playlistId;
            playlistItemsRequest.MaxResults = 50;

            var videos = new List<VideoInfo>();

            // CHANGED: replaced "while (playlistItemsRequest != null)" with "while (true)"
            while (true)
            {
                var playlistItemsResponse = await playlistItemsRequest.ExecuteAsync();

                foreach (var item in playlistItemsResponse.Items)
                {
                    string videoId = item.ContentDetails.VideoId;
                    string title = item.Snippet.Title;
                    string url = $"https://www.youtube.com/watch?v={videoId}";
                    string channelName = item.Snippet.ChannelTitle;

                    string duration = await GetVideoDuration(googleYouTube, videoId);

                    videos.Add(new VideoInfo
                    {
                        Title = title,
                        Channel = channelName,
                        Url = url,
                        Duration = duration
                    });
                }

                // CHANGED: Added a check to break when there's no next page
                if (string.IsNullOrEmpty(playlistItemsResponse.NextPageToken))
                {
                    break;
                }

                playlistItemsRequest.PageToken = playlistItemsResponse.NextPageToken;
            }

            var result = new
            {
                PlaylistTitle = playlistTitle,
                Videos = videos
            };

            return JsonConvert.SerializeObject(result, Formatting.Indented);
        }

        private static async Task<string> GetVideoDuration(Google.Apis.YouTube.v3.YouTubeService googleYouTube, string videoId)
        {
            var videoRequest = googleYouTube.Videos.List("contentDetails");
            videoRequest.Id = videoId;

            var videoResponse = await videoRequest.ExecuteAsync();

            if (videoResponse.Items.Count > 0)
            {
                return videoResponse.Items[0].ContentDetails.Duration;
            }

            return "Unknown";
        }
    }

    public class VideoInfo
    {
        public string Title { get; set; } = string.Empty;
        public string Channel { get; set; } = string.Empty;
        public string Url { get; set; } = string.Empty;
        public string Duration { get; set; } = string.Empty;
    }
}
