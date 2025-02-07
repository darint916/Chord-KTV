namespace ChordKTV.Services.Service;

using System;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using System.Globalization;
using ChordKTV.Services.Api;
using ChordKTV.Dtos;

public class LrcService : ILrcService
{
    private readonly HttpClient _httpClient;

    public LrcService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<LrcLyricsDto?> GetLrcLibLyricsAsync(string title, string artist, string? albumName, float? duration)
    {
        string query = $"track_name={Uri.EscapeDataString(title)}";
        query += $"&artist_name={Uri.EscapeDataString(artist)}";

        if (!string.IsNullOrEmpty(albumName))
        {
            query += $"&album_name={Uri.EscapeDataString(albumName)}";
        }

        if (duration.HasValue)
        {
            query += $"&duration={duration.Value.ToString(CultureInfo.InvariantCulture)}";
        }

        HttpResponseMessage response = await _httpClient.GetAsync($"https://lrclib.net/api/get?{query}");

        if (response.IsSuccessStatusCode)
        {
            string content = await response.Content.ReadAsStringAsync();
            JsonElement apiResponse = JsonSerializer.Deserialize<JsonElement>(content);

            LrcLyricsDto lrcLyricsDto = new LrcLyricsDto(
                LrcLibId: apiResponse.GetProperty("id").GetInt32(),
                Name: apiResponse.GetProperty("name").GetString() ?? "",
                TrackName: apiResponse.GetProperty("trackName").GetString() ?? "",
                ArtistName: apiResponse.GetProperty("artistName").GetString() ?? "",
                AlbumName: apiResponse.GetProperty("albumName").GetString() ?? "",
                Duration: TimeSpan.FromSeconds(apiResponse.GetProperty("duration").GetSingle()),
                Instrumental: apiResponse.GetProperty("instrumental").GetBoolean(),
                PlainLyrics: apiResponse.GetProperty("plainLyrics").GetString() ?? "",
                SyncedLyrics: apiResponse.GetProperty("syncedLyrics").GetString() ?? "",
                Romanized: false
            );

            return lrcLyricsDto;
        }

        return null;
    }
}
