using System.Text.Json;
using System.Text.Json.Serialization;
using ChordKTV.Services.Api;
using ChordKTV.Dtos;
using FuzzySharp;
using System.Web;
using System.Globalization;
using System.Collections.Specialized;
using System.Diagnostics;
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
        }

        List<LrcLyricsDto>? searchResults = await GetLrcLibLyricsListAsync(title, artist, albumName);
        if (searchResults is null)
        {
            return lyricsDtoMatch;
        }

        //We fuzzy order to add layer of confidence
        //TODO: remove null check if #70 issue resolved
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
        if (lyricsDtoMatch == null)
        {
            return null;
        }

        // we go more strict on time sync if we have a match
        if (!LanguageUtils.IsRomanizedText(lyricsDtoMatch.PlainLyrics)) // If lyrics are in non en language
        {
            LrcLyricsDto? romanizedMatch = searchResults.FirstOrDefault(ele =>
                !string.IsNullOrEmpty(ele.PlainLyrics) &&
                !string.IsNullOrEmpty(ele.SyncedLyrics) &&
                LanguageUtils.IsRomanizedText(ele.PlainLyrics) &&
                Fuzz.Ratio(ele.ArtistName, lyricsDtoMatch.ArtistName) > 80 && //make sure artist is same
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
            string? romanizedPlainLyrics = lyricsDtoMatch.PlainLyrics;
            string? romanizedSyncedLyrics = lyricsDtoMatch.SyncedLyrics;
            int? romanizedId = lyricsDtoMatch.Id;

            LrcLyricsDto? origMatch = searchResults.FirstOrDefault(ele =>
                !string.IsNullOrEmpty(ele.PlainLyrics) &&
                !string.IsNullOrEmpty(ele.SyncedLyrics) &&
                !LanguageUtils.IsRomanizedText(ele.PlainLyrics) &&
                Fuzz.Ratio(ele.ArtistName, lyricsDtoMatch.ArtistName) > 80 && //make sure artist is same
                CompareUtils.IsCloseToF(ele.Duration, duration, 2));

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

        LrcLyricsDto? result = null;
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
    public async Task<List<LrcLyricsDto>?> GetLrcLibLyricsListAsync(string? title, string? artist, string? albumName, bool forceKeywordSearch = false)
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
        //They mentioned this was slower, so maybe batch call
        //after testing, it seems their search prioritizes the query string over title and artist (if artist title corr, query string can kill it)
        // im p sure they anything in their qstring just searches all fields, we get from genius so might as well be specific
        // Note that we return on find results, i believe their search algorithm doesn't stray off too far from the query
        List<LrcLyricsDto>? results;
        if (!string.IsNullOrEmpty(artist) && !string.IsNullOrEmpty(albumName) && !forceKeywordSearch)
        {
            results = await LrcLibSearchEndpointResponse(queryParams);
            if (results is not null)
            {
                return results;
            }
        }
        if (!string.IsNullOrEmpty(artist) && !forceKeywordSearch) //artist + title query (no album)
        {
            queryParams.Remove("album_name");
            results = await LrcLibSearchEndpointResponse(queryParams);
            if (results is not null)
            {
                return results;
            }
        }
        //album no artist is wild so we dont cover

        //raw title query
        queryParams.Remove("artist_name");
        queryParams.Remove("album_name");
        if (!forceKeywordSearch)
        {
            results = await LrcLibSearchEndpointResponse(queryParams);
            if (results is not null)
            {
                return results;
            }
        }
        // time for a strip search, need to fine tune based off youtube results/ other names
        // we can use fuzzy search to find the best match
        //turns out LRCLib uses strip type too? (G)I-DLE -> G I Dle works
        string? keywords = KeywordExtractorUtils.ExtractSongKeywords(title, artist); //for now i think this is enough
        if (string.IsNullOrWhiteSpace(keywords))
        {
            return null; //wtf is this song
        }
        //we might need a regular query call with just title + artist? not sure though (prob not needed), so i'll leave it out

        // as of 2/19/2025, LRC Lib doesnt care about title + artist if we give qString
        // Fuzzy search for the best match
        queryParams["q"] = keywords;
        List<LrcLyricsDto>? allResults = await LrcLibSearchEndpointResponse(queryParams);
        if (allResults is null)
        {
            // Would want to personally see what we miss
            _logger.LogWarning("No results found for title: {Title}, artist: {Artist}, album: {AlbumName}", title, artist, albumName);
            _logger.LogWarning("Keywords: {Keywords}", keywords);
        }
        return allResults;
    }

    //api endpoint for LRCLib exact Get match https://lrclib.net/docs
    public async Task<LrcLyricsDto?> LrcLibGetEndpointResponse(NameValueCollection queryParams)
    {
        Stopwatch stopwatch = new(); //remove when done
        stopwatch.Start();
        string queryString = queryParams.ToString() ?? string.Empty; //empty string (intellisense is dumb)
        string url = $"https://lrclib.net/api/get?{queryString}";

        HttpResponseMessage response = await _httpClient.GetAsync(url);
        response.EnsureSuccessStatusCode();
        string content = await response.Content.ReadAsStringAsync();
        _logger.LogDebug("LrcLib API call took: {ElapsedMilliseconds}ms", stopwatch.ElapsedMilliseconds);
        return JsonSerializer.Deserialize<LrcLyricsDto>(content, _jsonSerializerOptions);
    }

    //api endpoint for LRCLib search https://lrclib.net/docs
    public async Task<List<LrcLyricsDto>?> LrcLibSearchEndpointResponse(NameValueCollection queryParams)
    {
        string queryString = queryParams.ToString() ?? string.Empty;
        HttpResponseMessage response = await _httpClient.GetAsync($"https://lrclib.net/api/search?{queryString}");
        response.EnsureSuccessStatusCode();
        string content = await response.Content.ReadAsStringAsync();
        List<LrcLyricsDto>? searchResults = JsonSerializer.Deserialize<List<LrcLyricsDto>?>(content, _jsonSerializerOptions);
        return (searchResults is { Count: > 0 }) ? searchResults : null;
    }
}
