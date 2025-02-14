using System.Globalization;
using System.Text.Json;
using System.Text.Json.Serialization;
using ChordKTV.Services.Api;
using ChordKTV.Dtos;

namespace ChordKTV.Services.Service;

public class LrcService : ILrcService
{
    private readonly HttpClient _httpClient;

    public LrcService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    private static bool IsRomanized(string? lyrics)
    {
        if (string.IsNullOrWhiteSpace(lyrics))
        {
            return false; // Assume empty input is in original lang
        }

        // Return true only if all characters are Latin
        return lyrics.All(IsLatinCharacter);
    }

    private static bool IsLatinCharacter(char c) => c switch
    {
        >= '\u0000' and <= '\u024F' => true, // Latin scripts: ASCII, Latin-1, Extended-A & B
        _ => false
    };

    private static readonly JsonSerializerOptions _jsonSerializerOptions = new()
    {
        // Make property names case-insensitive
        PropertyNameCaseInsensitive = true,
        // Ignore null values during deserialization
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
    };

    public async Task<LrcLyricsDto?> GetLrcLibLyricsAsync(string? title, string? artist, string? albumName, string? qString, float? duration)
    {
        string query = "";

        if (!string.IsNullOrEmpty(title))
        {
            query += $"&track_name={Uri.EscapeDataString(title)}";
        }

        if (!string.IsNullOrEmpty(artist))
        {
            query += $"&artist_name={Uri.EscapeDataString(artist)}";
        }

        if (!string.IsNullOrEmpty(albumName))
        {
            query += $"&album_name={Uri.EscapeDataString(albumName)}";
        }

        if (!string.IsNullOrEmpty(qString))
        {
            query += $"&q={Uri.EscapeDataString(qString)}";
        }

        HttpResponseMessage response = await _httpClient.GetAsync($"https://lrclib.net/api/search?{query}");
        if (response.IsSuccessStatusCode)
        {
            string content = await response.Content.ReadAsStringAsync();
            List<LrcLyricsDto>? searchResults = JsonSerializer.Deserialize<List<LrcLyricsDto>>(content, _jsonSerializerOptions);

            if (searchResults is not { Count: > 0 })
            {
                return null;
            }

            // Get the first result with time-synced lyrics
            LrcLyricsDto? lyricsDtoMatch = searchResults.FirstOrDefault(ele =>
                !string.IsNullOrEmpty(ele.PlainLyrics) &&
                !string.IsNullOrEmpty(ele.SyncedLyrics) &&
                (!duration.HasValue || ele.Duration == duration));

            if (lyricsDtoMatch == null)
            {
                return null;
            }

            if (!IsRomanized(lyricsDtoMatch.PlainLyrics)) // If lyrics are in original language
            {
                LrcLyricsDto? romanizedMatch = searchResults.FirstOrDefault(ele =>
                    !string.IsNullOrEmpty(ele.PlainLyrics) &&
                    !string.IsNullOrEmpty(ele.SyncedLyrics) &&
                    IsRomanized(ele.PlainLyrics) &&
                    (!duration.HasValue || ele.Duration == duration));

                if (romanizedMatch != null)
                {
                    lyricsDtoMatch.RomanizedPlainLyrics = romanizedMatch.PlainLyrics;
                    lyricsDtoMatch.RomanizedSyncedLyrics = romanizedMatch.SyncedLyrics;
                    lyricsDtoMatch.RomanizedId = romanizedMatch.Id;
                }
            }
            else // If already romanized, look for original lyrics
            {
                string? romanizedPlainLyrics = lyricsDtoMatch.PlainLyrics;
                string? romanizedSyncedLyrics = lyricsDtoMatch.SyncedLyrics;
                int? romanizedId = lyricsDtoMatch.Id;

                LrcLyricsDto? origMatch = searchResults.FirstOrDefault(ele =>
                    !string.IsNullOrEmpty(ele.PlainLyrics) &&
                    !string.IsNullOrEmpty(ele.SyncedLyrics) &&
                    !IsRomanized(ele.PlainLyrics) &&
                    (!duration.HasValue || ele.Duration == duration));

                if (origMatch != null)
                {
                    lyricsDtoMatch.PlainLyrics = origMatch.PlainLyrics;
                    lyricsDtoMatch.SyncedLyrics = origMatch.SyncedLyrics;
                    lyricsDtoMatch.Id = origMatch.Id;
                }

                lyricsDtoMatch.RomanizedPlainLyrics = romanizedPlainLyrics;
                lyricsDtoMatch.RomanizedSyncedLyrics = romanizedSyncedLyrics;
                lyricsDtoMatch.RomanizedId = romanizedId;
            }

            return lyricsDtoMatch;
        }

        return null;
    }
}
