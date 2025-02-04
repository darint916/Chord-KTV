namespace ChordKTV.Services;

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Google.Apis.Services;
using Google.Apis.YouTube.v3;
using Google.Apis.YouTube.v3.Data;
using Microsoft.Extensions.Configuration;
using ChordKTV.Models.ApiModels;
using ChordKTV.Services.Api;

public class YouTubeApiClient : IYouTubeService
{
    private readonly string _apiKey;

    public YouTubeApiClient(IConfiguration configuration)
    {
        _apiKey = configuration["YouTube:ApiKey"] ?? throw new InvalidOperationException("YouTube API key not configured.");
    }

    public async Task<PlaylistDetailsDto> GetPlaylistDetails(string playlistId)
    {
        YouTubeService googleYouTube = new YouTubeService(
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

        List<VideoInfo> videos = new List<VideoInfo>();
        PlaylistItemListResponse playlistItemsResponse;
        do
        {
            playlistItemsResponse = await playlistItemsRequest.ExecuteAsync();

            foreach (PlaylistItem item in playlistItemsResponse.Items)
            {
                string videoId = item.ContentDetails.VideoId;
                string title = item.Snippet.Title;
                string url = $"https://www.youtube.com/watch?v={videoId}";
                string channelName = item.Snippet.ChannelTitle;

                TimeSpan duration = await GetVideoDuration(googleYouTube, videoId);

                videos.Add(new VideoInfo(title, channelName, url, duration));
            }

            playlistItemsRequest.PageToken = playlistItemsResponse.NextPageToken;
        }
        while (!string.IsNullOrEmpty(playlistItemsRequest.PageToken));

        return new PlaylistDetailsDto(playlistTitle, videos);
    }

    private static async Task<TimeSpan> GetVideoDuration(YouTubeService googleYouTube, string videoId)
    {
        VideosResource.ListRequest videoRequest = googleYouTube.Videos.List("contentDetails");
        videoRequest.Id = videoId;

        VideoListResponse videoResponse = await videoRequest.ExecuteAsync();

        if (videoResponse.Items.Count > 0)
        {
            string isoDuration = videoResponse.Items[0].ContentDetails.Duration;
            try
            {
                // Convert ISO 8601 duration to TimeSpan
                TimeSpan duration = System.Xml.XmlConvert.ToTimeSpan(isoDuration);
                return duration;
            }
            catch
            {
                return TimeSpan.Zero;
            }
        }

        return TimeSpan.Zero;
    }
}
