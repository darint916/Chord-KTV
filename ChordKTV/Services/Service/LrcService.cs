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
            LrcLyricsDto? lrcLyricsDto = JsonSerializer.Deserialize<LrcLyricsDto>(content, _jsonSerializerOptions);
            return lrcLyricsDto;
        }

        return null;
    }

    public async Task<LrcLyricsDto?> GetLrcRomanizedLyricsAsync(LrcLyricsDto lyricsDto)
    {
        string query = "";

        if (!string.IsNullOrEmpty(lyricsDto.TrackName))
        {
            query += $"track_name={Uri.EscapeDataString(lyricsDto.TrackName)}";
        }

        if (!string.IsNullOrEmpty(lyricsDto.ArtistName))
        {
            query += $"&artist_name={Uri.EscapeDataString(lyricsDto.ArtistName)}";
        }

        HttpResponseMessage response = await _httpClient.GetAsync($"https://lrclib.net/api/search?{query}");
        if (response.IsSuccessStatusCode)
        {
            string content = await response.Content.ReadAsStringAsync();
            List<JsonElement>? searchResults = JsonSerializer.Deserialize<List<JsonElement>>(content);

            if (searchResults is { Count: > 0 })
            {
                if (!IsRomanized(lyricsDto.PlainLyrics)) // If lyrics provided are in original lang
                {
                    // Look for any result that has romanized lyrics
                    JsonElement? romanizedMatch = searchResults
                        .FirstOrDefault(ele =>
                        {
                            string plainLyrics = ele.GetProperty("plainLyrics").GetString() ?? "";
                            return plainLyrics != null && IsRomanized(plainLyrics);
                        });

                    if (romanizedMatch != null && romanizedMatch.Value.ValueKind != JsonValueKind.Undefined)
                    {
                        lyricsDto.RomanizedPlainLyrics = romanizedMatch.Value.GetProperty("plainLyrics").GetString();
                        lyricsDto.RomanizedSyncedLyrics = romanizedMatch.Value.GetProperty("syncedLyrics").GetString();
                        return lyricsDto;
                    }
                }
                else // If already romanized, look for original lang
                {
                    string? romanizedPlainLyrics = lyricsDto.PlainLyrics;
                    string? romanizedSyncedLyrics = lyricsDto.SyncedLyrics;

                    // Look for any other result that has original lyrics
                    JsonElement? origMatch = searchResults
                        .FirstOrDefault(ele =>
                        {
                            string plainLyrics = ele.GetProperty("plainLyrics").GetString() ?? "";
                            return plainLyrics != null && !IsRomanized(plainLyrics);
                        });


                    if (origMatch != null && origMatch.Value.ValueKind != JsonValueKind.Undefined)
                    {
                        lyricsDto.PlainLyrics = origMatch.Value.GetProperty("plainLyrics").GetString();
                        lyricsDto.SyncedLyrics = origMatch.Value.GetProperty("syncedLyrics").GetString();
                        lyricsDto.RomanizedPlainLyrics = romanizedPlainLyrics;
                        lyricsDto.RomanizedSyncedLyrics = romanizedSyncedLyrics;
                    }
                    else // In this case, song was originally English
                    {
                        lyricsDto.RomanizedPlainLyrics = romanizedPlainLyrics;
                        lyricsDto.RomanizedSyncedLyrics = romanizedSyncedLyrics;
                    }

                    return lyricsDto;
                }
            }
        }

        return null;
    }
}
