namespace ChordKTV.Services.Service;

using System;
using System.Net.Http;
using System.Threading.Tasks;
using System.Globalization;
using ChordKTV.Services.Api;

public class LrcService : ILrcService
{
    private readonly HttpClient _httpClient;

    public LrcService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<string?> GetLrcLibLyricsAsync(string title, string artist, string? albumName, float? duration)
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

            return content;
        }

        return null;
    }
}
