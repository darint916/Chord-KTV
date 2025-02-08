namespace ChordKTV.Services.Service;

using System;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using System.Globalization;
using System.Linq;
using ChordKTV.Services.Api;
using ChordKTV.Dtos;

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
            return true; // Assume empty input is romanized
        }

        // Return true only if all characters are Latin
        return lyrics.All(IsLatinCharacter);
    }

    private static bool IsLatinCharacter(char c) => c switch
    {
        >= '\u0000' and <= '\u024F' => true, // Latin scripts: ASCII, Latin-1, Extended-A & B
        _ => false
    };

    public async Task<LrcLyricsDto?> GetLrcLibLyricsAsync(string title, string? artist, string? albumName)
    {
        string query = $"track_name={Uri.EscapeDataString(title)}";

        if (!string.IsNullOrEmpty(artist))
        {
            query += $"&artist_name={Uri.EscapeDataString(artist)}";
        }

        if (!string.IsNullOrEmpty(albumName))
        {
            query += $"&album_name={Uri.EscapeDataString(albumName)}";
        }

        HttpResponseMessage response = await _httpClient.GetAsync($"https://lrclib.net/api/search?{query}");

        if (response.IsSuccessStatusCode)
        {
            string content = await response.Content.ReadAsStringAsync();
            var searchResults = JsonSerializer.Deserialize<List<JsonElement>>(content);

            if (searchResults is { Count: > 0 })
            {
                // Take the first result as the primary match
                var bestMatch = MapToDto(searchResults.First());

                if (!IsRomanized(bestMatch.PlainLyrics)) // If lyrics in first result are in original lang
                {
                    // Look for any other result that has romanized lyrics
                    var romanizedMatch = searchResults
                        .Skip(1) // Skip the first since it's already selected
                        .Select(MapToDto)
                        .FirstOrDefault(dto => IsRomanized(dto.PlainLyrics));

                    if (romanizedMatch != null)
                    {
                        bestMatch = bestMatch with
                        {
                            RomanizedPlainLyrics = romanizedMatch.PlainLyrics,
                            RomanizedSyncedLyrics = romanizedMatch.SyncedLyrics
                        };
                    }
                }
                else // If already romanized, look for original lang
                {
                    string romanizedPlainLyrics = bestMatch.PlainLyrics;
                    string romanizedSyncedLyrics = bestMatch.SyncedLyrics;

                    // Look for any other result that has original lyrics
                    var origMatch = searchResults
                        .Skip(1) // Skip the first since it's already selected
                        .Select(MapToDto)
                        .FirstOrDefault(dto => !IsRomanized(dto.PlainLyrics));

                    if (origMatch != null)
                    {
                        bestMatch = bestMatch with
                        {
                            PlainLyrics = origMatch.PlainLyrics,
                            SyncedLyrics = origMatch.SyncedLyrics,
                            RomanizedPlainLyrics = romanizedPlainLyrics,
                            RomanizedSyncedLyrics = romanizedSyncedLyrics
                        };
                    }
                    else // In this case, song was originally English
                    {
                        bestMatch = bestMatch with
                        {
                            RomanizedPlainLyrics = romanizedPlainLyrics,
                            RomanizedSyncedLyrics = romanizedSyncedLyrics
                        };
                    }
                }

                return bestMatch;
            }
        }

        return null;
    }

    private LrcLyricsDto MapToDto(JsonElement json)
    {
        return new LrcLyricsDto(
            LrcLibId: json.GetProperty("id").GetInt32(),
            Name: json.GetProperty("name").GetString() ?? "",
            TrackName: json.GetProperty("trackName").GetString() ?? "",
            ArtistName: json.GetProperty("artistName").GetString() ?? "",
            AlbumName: json.GetProperty("albumName").GetString() ?? "",
            Duration: TimeSpan.FromSeconds(json.GetProperty("duration").GetSingle()),
            Instrumental: json.GetProperty("instrumental").GetBoolean(),
            PlainLyrics: json.GetProperty("plainLyrics").GetString() ?? "",
            SyncedLyrics: json.GetProperty("syncedLyrics").GetString() ?? "",
            RomanizedPlainLyrics: null,  // Will be checked later
            RomanizedSyncedLyrics: null
        );
    }
}
