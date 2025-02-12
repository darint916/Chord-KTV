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
            List<JsonElement>? searchResults = JsonSerializer.Deserialize<List<JsonElement>>(content);

            if (searchResults is { Count: > 0 })
            {
                // Get the first result with time synced lyrics
                JsonElement? lyricsDtoMatch = searchResults.FirstOrDefault(ele =>
                {
                    string plainLyrics = ele.GetProperty("plainLyrics").GetString() ?? "";
                    string syncedLyrics = ele.GetProperty("syncedLyrics").GetString() ?? "";
                    float duration_ = ele.GetProperty("duration").GetSingle();
                    bool durationMatch = true;
                    // If the duration field was given, check if entry has same duration (defaults to true if no duration given)
                    if (duration.HasValue && duration_ != duration)
                    {
                        durationMatch = false;
                    }
                    return plainLyrics != null && syncedLyrics != null && durationMatch;
                });

                if (lyricsDtoMatch == null)
                {
                    return null;
                }
                LrcLyricsDto? lyricsDto = JsonSerializer.Deserialize<LrcLyricsDto>(lyricsDtoMatch.Value.ToString(), _jsonSerializerOptions);

                if (lyricsDto == null) // Shouldn't happen if lyricsDtoMatch wasn't null, throwing this check in for linter
                {
                    return null;
                }

                if (!IsRomanized(lyricsDto.PlainLyrics)) // If lyrics provided are in original lang
                {
                    // Look for any result that has romanized lyrics
                    JsonElement? romanizedMatch = searchResults
                        .FirstOrDefault(ele =>
                        {
                            string plainLyrics = ele.GetProperty("plainLyrics").GetString() ?? "";
                            string syncedLyrics = ele.GetProperty("syncedLyrics").GetString() ?? "";
                            float duration_ = ele.GetProperty("duration").GetSingle();
                            bool durationMatch = true;
                            // If the duration field was given, check if entry has same duration (defaults to true if no duration given)
                            if (duration.HasValue && duration_ != duration)
                            {
                                durationMatch = false;
                            }
                            return plainLyrics != null && IsRomanized(plainLyrics) && syncedLyrics != null && durationMatch;
                        });

                    if (romanizedMatch != null && romanizedMatch.Value.ValueKind != JsonValueKind.Undefined)
                    {
                        lyricsDto.RomanizedPlainLyrics = romanizedMatch.Value.GetProperty("plainLyrics").GetString();
                        lyricsDto.RomanizedSyncedLyrics = romanizedMatch.Value.GetProperty("syncedLyrics").GetString();
                        lyricsDto.RomanizedId = romanizedMatch.Value.GetProperty("id").GetInt32();
                    }
                    return lyricsDto;
                }
                else // If already romanized, look for original lang
                {
                    string? romanizedPlainLyrics = lyricsDto.PlainLyrics;
                    string? romanizedSyncedLyrics = lyricsDto.SyncedLyrics;
                    int? romanizedId = lyricsDto.Id;

                    // Look for any other result that has original lyrics
                    JsonElement? origMatch = searchResults
                        .FirstOrDefault(ele =>
                        {
                            string plainLyrics = ele.GetProperty("plainLyrics").GetString() ?? "";
                            string syncedLyrics = ele.GetProperty("syncedLyrics").GetString() ?? "";
                            float duration_ = ele.GetProperty("duration").GetSingle();
                            bool durationMatch = true;
                            // If the duration field was given, check if entry has same duration (defaults to true if no duration given)
                            if (duration.HasValue && duration_ != duration)
                            {
                                durationMatch = false;
                            }
                            return plainLyrics != null && !IsRomanized(plainLyrics) && syncedLyrics != null && durationMatch;
                        });


                    if (origMatch != null && origMatch.Value.ValueKind != JsonValueKind.Undefined)
                    {
                        lyricsDto.PlainLyrics = origMatch.Value.GetProperty("plainLyrics").GetString();
                        lyricsDto.SyncedLyrics = origMatch.Value.GetProperty("syncedLyrics").GetString();
                        lyricsDto.Id = origMatch.Value.GetProperty("id").GetInt32();
                    }

                    lyricsDto.RomanizedPlainLyrics = romanizedPlainLyrics;
                    lyricsDto.RomanizedSyncedLyrics = romanizedSyncedLyrics;
                    lyricsDto.RomanizedId = romanizedId;

                    return lyricsDto;
                }
            }
        }

        return null;
    }
}
