namespace ChordKTV.Services.Service;

using System;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using ChordKTV.Services.Api;
using ChordKTV.Dtos;

public partial class LrcService : ILrcService
{
    private readonly HttpClient _httpClient;

    public LrcService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    [GeneratedRegex(@"[^\p{IsBasicLatin}\p{IsLatin-1Supplement}]")]
    private static partial Regex NonLatinRegex(); // Generates the regex at compile-time

    private static bool IsRomanized(string? lyrics, double threshold = 0.3)
    {
        if (string.IsNullOrWhiteSpace(lyrics))
        {
            return true;
        }
        // Count total characters
        int totalChars = lyrics.Length;

        // Regex to match non-Latin characters
        Regex nonLatinRegex = NonLatinRegex();

        // Count non-Latin characters
        int nonLatinCount = nonLatinRegex.Matches(lyrics).Count;

        // If non-Latin characters make up a significant portion, assume original script
        return (nonLatinCount / (double)totalChars) < threshold;
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
                Romanized: IsRomanized(apiResponse.GetProperty("plainLyrics").GetString())
            );

            return lrcLyricsDto;
        }

        return null;
    }
}
