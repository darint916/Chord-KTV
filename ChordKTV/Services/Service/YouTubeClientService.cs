namespace ChordKTV.Services;

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Google.Apis.Services;
using Google.Apis.YouTube.v3;
using Google.Apis.YouTube.v3.Data;
using ChordKTV.Services.Api;
using ChordKTV.Dtos;

public class YouTubeApiClientService : IYouTubeClientService
{
    private readonly string? _apiKey;

    public YouTubeApiClientService(IConfiguration configuration)
    {
        _apiKey = configuration["YouTube:ApiKey"];
    }

    public async Task<PlaylistDetailsDto?> GetPlaylistDetailsAsync(string playlistId)
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

            foreach (PlaylistItem item in playlistItemsResponse.Items)
            {
                string videoId = item.ContentDetails.VideoId;
                string title = item.Snippet.Title;
                string url = $"https://www.youtube.com/watch?v={videoId}";
                string channelName = item.Snippet.ChannelTitle;

                TimeSpan duration = await GetVideoDurationAsync(googleYouTube, videoId);

                videos.Add(new VideoInfo(title, channelName, url, duration));
            }

            playlistItemsRequest.PageToken = playlistItemsResponse.NextPageToken;
        }
        while (!string.IsNullOrEmpty(playlistItemsRequest.PageToken));

        return new PlaylistDetailsDto(playlistTitle, videos);
    }

    private static async Task<TimeSpan> GetVideoDurationAsync(YouTubeService googleYouTube, string videoId)
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
                var duration = System.Xml.XmlConvert.ToTimeSpan(isoDuration);
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
