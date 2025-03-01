using System.Text.Json;
using System.Text.Json.Serialization;
using ChordKTV.Services.Api;
using ChordKTV.Dtos;
using System.Web;
using System.Globalization;
using System.Collections.Specialized;
using ChordKTV.Utils;

namespace ChordKTV.Services.Service;

public class LrcService : ILrcService
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<LrcService> _logger;
    private static readonly JsonSerializerOptions _jsonSerializerOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
    };
    public LrcService(HttpClient httpClient, ILogger<LrcService> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
    }
    public async Task<LrcLyricsDto?> GetAllLrcLibLyricsAsync(string? title, string? artist, string? albumName, float? duration)
    {
        NameValueCollection queryParams = HttpUtility.ParseQueryString(string.Empty);
        if (!string.IsNullOrEmpty(title))
        {
            queryParams["track_name"] = title;
        }
        else
        {
            throw new ArgumentException("At least title be provided");
        }
        if (!string.IsNullOrEmpty(artist))
        {
            queryParams["artist_name"] = artist;
        }

        if (!string.IsNullOrEmpty(albumName))
        {
            queryParams["album_name"] = albumName;
        }

        //do exact search first, following LRC Lib recommendation: https://lrclib.net/docs
        // then gets the list of songs and then finds the best match
        LrcLyricsDto? lyricsDtoMatch = null;
        if (!string.IsNullOrEmpty(title) && !string.IsNullOrEmpty(artist))
        {
            lyricsDtoMatch = await GetLrcLibLyricsExactAsync(title, artist, albumName, duration);
            if (lyricsDtoMatch is not null)
            {
                _logger.LogDebug("Found exact match for title: {Title}, artist: {Artist}, album: {AlbumName}", title, artist, albumName);
            }
        }

        List<LrcLyricsDto>? searchResults = await GetLrcLibLyricsListAsync(title, artist, albumName);
        if (searchResults is null)
        {
            return lyricsDtoMatch;
        }

        //We fuzzy order to add layer of confidence
        searchResults = searchResults
            .OrderByDescending(ele => CompareUtils
            .CompareWeightedFuzzyScore(title, ele.TrackName ?? "", artist, ele.ArtistName, duration, ele.Duration))
            .ToList();

        // Get the first result with time-synced lyrics
        if (string.IsNullOrWhiteSpace(lyricsDtoMatch?.SyncedLyrics))
        {
            lyricsDtoMatch = searchResults.FirstOrDefault(ele =>
                !string.IsNullOrEmpty(ele.PlainLyrics) &&
                !string.IsNullOrEmpty(ele.SyncedLyrics));
        }

        // Collect alternate titles and artists from search results
        if (lyricsDtoMatch != null && searchResults.Count > 0)
        {
            // Get unique titles and artists that meet minimum similarity threshold
            foreach (LrcLyricsDto result in searchResults.Take(5)) // Limit to top 5 results
            {
                if (!string.IsNullOrWhiteSpace(result.TrackName) &&
                    !lyricsDtoMatch.AlternateTitles.Contains(result.TrackName))
                {
                    lyricsDtoMatch.AlternateTitles.Add(result.TrackName);
                }

                if (!string.IsNullOrWhiteSpace(result.ArtistName) &&
                    CompareUtils.CompareArtistFuzzyScore(lyricsDtoMatch.ArtistName, result.ArtistName) > 80)
                {
                    lyricsDtoMatch.AlternateArtists.Add(result.ArtistName);
                }
            }
        }

        if (lyricsDtoMatch == null)
        {
            //if we fail to find even a single match, just grab first entry if it exists
            if (searchResults.Count > 0)
            {
                return searchResults[0];
            }
            return null;
        }

        // we go more strict on time sync if we have a match, note we prioritize LRC
        if (!LanguageUtils.IsRomanizedText(lyricsDtoMatch.PlainLyrics)) // If lyrics are in non en language
        {
            LrcLyricsDto? romanizedMatch = searchResults.FirstOrDefault(ele =>
                !string.IsNullOrEmpty(ele.SyncedLyrics) &&
                LanguageUtils.IsRomanizedText(ele.PlainLyrics) &&
                CompareUtils.CompareArtistFuzzyScore(artist, ele.ArtistName, lyricsDtoMatch.ArtistName, 90) > 80 && //make sure artist is same
                CompareUtils.IsCloseToF(ele.Duration, duration, 2));

            if (romanizedMatch != null)
            {
                lyricsDtoMatch.RomanizedPlainLyrics = romanizedMatch.PlainLyrics;
                lyricsDtoMatch.RomanizedSyncedLyrics = romanizedMatch.SyncedLyrics;
                lyricsDtoMatch.RomanizedId = romanizedMatch.Id;
            }
        }
        else // If already romanized, look for original lyrics
        {
            lyricsDtoMatch.RomanizedPlainLyrics = lyricsDtoMatch.PlainLyrics;
            lyricsDtoMatch.RomanizedSyncedLyrics = lyricsDtoMatch.SyncedLyrics;
            lyricsDtoMatch.RomanizedId = lyricsDtoMatch.Id;
            LrcLyricsDto? origMatch = searchResults.FirstOrDefault(ele =>
                !string.IsNullOrEmpty(ele.SyncedLyrics) &&
                !LanguageUtils.IsRomanizedText(ele.PlainLyrics) &&
                CompareUtils.CompareArtistFuzzyScore(artist, ele.ArtistName, lyricsDtoMatch.ArtistName, 90) > 80 && //make sure artist is same
                CompareUtils.IsCloseToF(ele.Duration, duration, 2));

            if (origMatch != null)
            {
                lyricsDtoMatch.PlainLyrics = origMatch.PlainLyrics;
                lyricsDtoMatch.SyncedLyrics = origMatch.SyncedLyrics;
                lyricsDtoMatch.Id = origMatch.Id;
            }
        }
        return lyricsDtoMatch;
    }

    //Most restrictive query, exact match of title and artist
    public async Task<LrcLyricsDto?> GetLrcLibLyricsExactAsync(string title, string artist, string? albumName, float? duration)
    {
        if (string.IsNullOrWhiteSpace(title) || string.IsNullOrWhiteSpace(artist))
        {
            throw new ArgumentException("Both track_name and artist_name are required.");
        }
        NameValueCollection queryParams = HttpUtility.ParseQueryString(string.Empty);
        queryParams["track_name"] = title;
        queryParams["artist_name"] = artist;
        string? durationString = duration?.ToString(CultureInfo.InvariantCulture) ?? null;

        LrcLyricsDto? result;
        //TODO: benchmark and consider pool api call?
        if (!string.IsNullOrWhiteSpace(albumName) && !string.IsNullOrWhiteSpace(durationString))
        {
            queryParams["duration"] = durationString;
            queryParams["album_name"] = albumName;
            result = await LrcLibGetEndpointResponse(queryParams);
            if (result is not null)
            {
                return result;
            }
        }
        if (!string.IsNullOrWhiteSpace(durationString))
        {
            queryParams["duration"] = durationString;
            queryParams.Remove("album_name");
            result = await LrcLibGetEndpointResponse(queryParams);
            if (result is not null)
            {
                return result;
            }
        }
        if (!string.IsNullOrWhiteSpace(albumName))
        {
            queryParams["album_name"] = albumName;
            queryParams.Remove("duration");
            result = await LrcLibGetEndpointResponse(queryParams);
            if (result is not null)
            {
                return result;
            }
        }

        //Least strict check with get endpoint, im pretty sure if there is a match, no need to fuzzy match
        queryParams.Remove("album_name");
        queryParams.Remove("duration");
        result = await LrcLibGetEndpointResponse(queryParams);
        return result;
    }

    //Searches for lyrics based on title, artist, and album name and other random fuzzy methods, add more if needed
    public async Task<List<LrcLyricsDto>?> GetLrcLibLyricsListAsync(string? title, string? artist, string? albumName)
    {
        NameValueCollection queryParams = HttpUtility.ParseQueryString(string.Empty);
        if (!string.IsNullOrEmpty(title))
        {
            queryParams["track_name"] = title;
        }
        else
        {
            throw new ArgumentException("At least title be provided");
        }
        if (!string.IsNullOrEmpty(artist))
        {
            queryParams["artist_name"] = artist;
        }

        if (!string.IsNullOrEmpty(albumName))
        {
            queryParams["album_name"] = albumName;
        }
        //after testing, it seems their search prioritizes the query string over title and artist (if artist title corr, query string can kill it)
        // im p sure they anything in their qstring just searches all fields, we get from genius so might as well be specific
        // Note that we return on find results, i believe their search algorithm doesn't stray off too far from the query
        //Initialize keywords search to append to results later

        // time for a strip search, need to fine tune based off youtube results/ other names
        // we can use fuzzy search to find the best match
        //turns out LRCLib uses strip type too? (G)I-DLE -> G I Dle works
        List<Task<List<LrcLyricsDto>?>> queryTasks = new(); //batch the calls in parallel, we force strip search for more results
        string? titleKeywords = KeywordExtractorUtils.ExtractSongKeywords(title); //for now i think this is enough
        string? artistKeywords = KeywordExtractorUtils.ExtractSongKeywords(artist);
        if (!string.IsNullOrWhiteSpace(titleKeywords)) //we query with both title, title+artist as artist may sometimes be bad match
        {
            NameValueCollection keywordParams = HttpUtility.ParseQueryString(string.Empty);
            keywordParams["q"] = titleKeywords;
            queryTasks.Add(LrcLibSearchEndpointResponse(keywordParams));
            if (!string.IsNullOrWhiteSpace(artistKeywords))
            {
                keywordParams["q"] = titleKeywords + " " + artistKeywords;
                queryTasks.Add(LrcLibSearchEndpointResponse(keywordParams));
            }
        }
        if (!string.IsNullOrEmpty(artist) && !string.IsNullOrEmpty(albumName))
        {
            queryTasks.Add(LrcLibSearchEndpointResponse(new NameValueCollection(queryParams))); //copy so we don't get race conditions in parallel
        }
        if (!string.IsNullOrEmpty(artist)) //artist + title query (no album)
        {
            queryParams.Remove("album_name");
            queryTasks.Add(LrcLibSearchEndpointResponse(new NameValueCollection(queryParams)));
        }
        //raw title query
        queryParams.Remove("artist_name");
        queryParams.Remove("album_name");
        queryTasks.Add(LrcLibSearchEndpointResponse(new NameValueCollection(queryParams))); //diff that keywords above, is more exact

        // Run all tasks in parallel and await them
        List<LrcLyricsDto> results = (await Task.WhenAll(queryTasks))
            .Where(list => list is not null)  // Remove null results
            .SelectMany(list => list!)        // Flatten into a single list
            .ToList();

        return results; //note we might get dupes, but i think sample size small enough to not worry (given 20 results per call, max 80)
    }

    //api endpoint for LRCLib exact Get match https://lrclib.net/docs
    public async Task<LrcLyricsDto?> LrcLibGetEndpointResponse(NameValueCollection queryParams)
    {
        string queryString = queryParams.ToString() ?? string.Empty; //empty string (intellisense is dumb)
        string url = $"https://lrclib.net/api/get?{queryString}";

        HttpResponseMessage response = await _httpClient.GetAsync(url);
        if (response.StatusCode == System.Net.HttpStatusCode.NotFound) //dont want it throw if not found, continue execution
        {
            return null;
        }
        response.EnsureSuccessStatusCode();
        string content = await response.Content.ReadAsStringAsync();
        return JsonSerializer.Deserialize<LrcLyricsDto>(content, _jsonSerializerOptions);
    }

    //api endpoint for LRCLib search https://lrclib.net/docs
    public async Task<List<LrcLyricsDto>?> LrcLibSearchEndpointResponse(NameValueCollection queryParams)
    {
        string queryString = queryParams.ToString() ?? string.Empty;
        HttpResponseMessage response = await _httpClient.GetAsync($"https://lrclib.net/api/search?{queryString}");
        if (response.StatusCode == System.Net.HttpStatusCode.NotFound) //dont want it throw if not found, continue execution
        {
            return null;
        }
        response.EnsureSuccessStatusCode();
        string content = await response.Content.ReadAsStringAsync();
        List<LrcLyricsDto>? searchResults = JsonSerializer.Deserialize<List<LrcLyricsDto>?>(content, _jsonSerializerOptions);
        return (searchResults is { Count: > 0 }) ? searchResults : null;
    }
}
