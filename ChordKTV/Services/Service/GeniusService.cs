using System.Net.Http.Headers;
using System.Text.Json;
using ChordKTV.Services.Api;
using ChordKTV.Data.Api.SongData;
using ChordKTV.Models.SongData;
using ChordKTV.Dtos;
using System.Globalization;
using ChordKTV.Dtos.GeniusApi;
using ChordKTV.Utils;
using Microsoft.AspNetCore.Components.Web;

namespace ChordKTV.Services.Service;

public class GeniusService : IGeniusService
{
    private readonly HttpClient _httpClient;
    private readonly ISongRepo _songRepo;
    private readonly IAlbumRepo _albumRepo;
    private readonly ILogger<GeniusService> _logger;
    private readonly string _accessToken;
    private const string BaseUrl = "https://api.genius.com";
    private static readonly JsonSerializerOptions _jsonOptions = new() { PropertyNameCaseInsensitive = true };
    private readonly IGeniusMetaDataRepo _geniusMetaDataRepo;

    public GeniusService(
        IConfiguration configuration,
        HttpClient httpClient,
        ISongRepo songRepo,
        IAlbumRepo albumRepo,
        ILogger<GeniusService> logger,
        IGeniusMetaDataRepo geniusMetaDataRepo)
    {
        _accessToken = configuration["Genius:ApiKey"] ??
            throw new ArgumentNullException(nameof(configuration), "Genius API key is required");
        _httpClient = httpClient;
        _songRepo = songRepo;
        _albumRepo = albumRepo;
        _logger = logger;
        _geniusMetaDataRepo = geniusMetaDataRepo;

        _httpClient.BaseAddress = new Uri(BaseUrl);
        _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _accessToken);
    }

    /// <summary>
    /// Helper to query Genius using the given search query and compare results with the provided fuzzy criteria
    /// </summary>
    private async Task<Song?> SearchGenius(string searchQuery, string? fuzzyTitle, string? fuzzyArtist)
    {
        _logger.LogInformation("Starting Genius search with query: '{Query}', title: '{Title}', artist: '{Artist}'",
            searchQuery, fuzzyTitle, fuzzyArtist);

        string requestUrl = $"/search?q={Uri.EscapeDataString(searchQuery)}";
        try
        {
            _logger.LogDebug("Sending request to: {Url}", requestUrl);
            HttpResponseMessage response = await _httpClient.GetAsync(requestUrl);

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogError("Genius API error: {StatusCode}", response.StatusCode);
                return null;
            }

            string responseContent = await response.Content.ReadAsStringAsync();
            _logger.LogDebug("Raw API Response: {Response}", responseContent);

            GeniusSearchResponse? searchResponse = JsonSerializer.Deserialize<GeniusSearchResponse>(
                responseContent, _jsonOptions);

            if (searchResponse?.Response?.Hits == null || searchResponse.Meta.Status != 200)
            {
                _logger.LogWarning("Invalid response structure or status. Meta Status: {Status}",
                    searchResponse?.Meta.Status);
                return null;
            }

            _logger.LogInformation("Retrieved {Count} hits from Genius", searchResponse.Response.Hits.Count);


            //lyric search assumed
            if (string.IsNullOrWhiteSpace(fuzzyTitle) && string.IsNullOrWhiteSpace(fuzzyArtist))
            {
                return await MapGeniusResultToSongAsync(searchResponse.Response.Hits[0].Result);
            }

            // Update the score check
            Dictionary<GeniusHit, int> hitScores = searchResponse.Response.Hits
                .Where(h => h.Result != null)
                .ToDictionary(
                    h => h,
                    h => CompareUtils.CompareWeightedFuzzyScore(fuzzyTitle ?? "", h.Result.Title, fuzzyArtist ?? "", h.Result.PrimaryArtistNames, 0, 0, artistDifferenceWeight: 0.3f)
                );

            //Order and filter titles
            List<GeniusHit> matches = hitScores
                .Where(dic => dic.Value > 70)
                .OrderByDescending(dic => dic.Value)
                .Select(dic => dic.Key)
                .ToList();


            //TODO: Later refactor maybe and return all the hit list to maybe be selectable options for user (to refine search query)
            //Do a subcheck to ensure it meets min artist requirements
            GeniusHit? bestMatch = null;
            if (!string.IsNullOrWhiteSpace(fuzzyArtist))
            {
                bestMatch = matches
                    .FirstOrDefault(h => CompareUtils
                        .CompareArtistFuzzyScore(fuzzyArtist, h.Result.PrimaryArtistNames) > 90);
            }
            else
            {   //if no artist is provided, we can use the title match, looser, still testing, use a custom title match if needed
                bestMatch = matches
                    .FirstOrDefault(h => CompareUtils
                        .CompareArtistFuzzyScore(fuzzyTitle, h.Result.Title, bonusWeight: 0.32) > 80);
            }


            if (bestMatch?.Result == null)
            {
                _logger.LogWarning("SearchGenius: No matches found for query: {SearchQuery} ; matching with Title: {FuzzyTitle} ; Artist: {FuzzyArtist}", searchQuery, fuzzyTitle, fuzzyArtist);
                return null;
            }
            return await MapGeniusResultToSongAsync(bestMatch.Result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "SearchGenius: Error fetching song from Genius API");
            return null;
        }
    }

    public async Task<Song?> GetSongByArtistTitleAsync(string? title, string? artist, string? lyrics, bool forceRefresh = false)
    {
        if (string.IsNullOrWhiteSpace(title) && string.IsNullOrWhiteSpace(artist) && string.IsNullOrWhiteSpace(lyrics))
        {
            return null;
        }

        _logger.LogInformation("Starting song search - Title: '{Title}', Artist: '{Artist}', ForceRefresh: {ForceRefresh}",
            title, artist, forceRefresh);

        if (!forceRefresh)
        {
            Song? cachedSong = await _songRepo.GetSongAsync(title ?? "", artist ?? "");
            if (cachedSong != null)
            {
                _logger.LogInformation("Found song in cache");
                return cachedSong;
            }
        }

        Song? result = null;

        if (!string.IsNullOrWhiteSpace(title))
        {
            // Try combined search without quotes first
            if (!string.IsNullOrEmpty(artist))
            {
                string combinedQuery = $"{title} {artist}";
                _logger.LogInformation("Attempting combined search with query: '{Query}'", combinedQuery);
                result = await SearchGenius(combinedQuery, title, artist);
            }

            // Fallback to title only if no result
            if (result == null)
            {
                _logger.LogInformation("Attempting title-only search with query: '{Query}'", title);
                result = await SearchGenius(title, title, artist);
            }
        }

        if (result == null && !string.IsNullOrWhiteSpace(lyrics))
        {
            _logger.LogInformation("Attempting lyrics search");
            result = await SearchGenius(lyrics, title, artist);
        }

        if (result != null)
        {
            result = await EnrichSongDetailsAsync(result);
            _logger.LogInformation("Found matching song. Title: '{Title}', Artist: '{Artist}'",
                result.Title, result.Artist);
        }
        else
        {
            _logger.LogWarning("GeniusService: GetSongByArtistTitleAsync: No matching song found after all search attempts");
        }
        return result;
    }

    public async Task<List<Song>> GetSongsByArtistTitleAsync(List<VideoInfo> videos, bool forceRefresh = false)
    {
        List<Song> songs = [];
        foreach (VideoInfo video in videos)
        {
            Song? song = await GetSongByArtistTitleAsync(video.Title, video.Artist, null, forceRefresh);
            if (song != null)
            {
                songs.Add(song);
            }
        }
        return songs;
    }

    private async Task<Song?> MapGeniusResultToSongAsync(GeniusResult result)
    {
        // First check if GeniusMetaData already exists
        GeniusMetaData? existingMetaData = await _geniusMetaDataRepo.GetGeniusMetaDataAsync(result.Id);

        GeniusMetaData metaData = existingMetaData ?? new GeniusMetaData
        {
            GeniusId = result.Id,
            HeaderImageUrl = result.HeaderImageUrl,
            SongImageUrl = result.SongArtImageUrl
        };

        // Create song object with existing or new metadata
        Song song = new()
        {
            Title = result.Title,
            Artist = result.PrimaryArtistNames,
            GeniusMetaData = metaData
        };

        // Handle album if present
        if (result.Album != null && !string.IsNullOrEmpty(result.Album.Name))
        {
            Album? existingAlbum = await _albumRepo.GetAlbumAsync(result.Album.Name, song.Artist);
            if (existingAlbum != null)
            {
                song.Albums.Add(existingAlbum);
            }
            else
            {
                song.Albums.Add(new Album
                {
                    Name = result.Album.Name,
                    Artist = song.Artist
                });
            }
        }

        return song;
    }

    public async Task<Song> EnrichSongDetailsAsync(Song song)
    {
        if (song.GeniusMetaData.GeniusId == 0)
        {
            return song;
        }

        string requestUrl = $"/songs/{song.GeniusMetaData.GeniusId}";

        try
        {
            HttpResponseMessage response = await _httpClient.GetAsync(requestUrl);
            response.EnsureSuccessStatusCode();

            GeniusSongResponse? songResponse = await response.Content.ReadFromJsonAsync<GeniusSongResponse>(
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            if (songResponse?.Meta.Status != 200)
            {
                return song;
            }

            GeniusSongDetails songDetails = songResponse.Response.Song;

            // Set language if available
            if (!string.IsNullOrEmpty(songDetails.Language))
            {
                string langStr = songDetails.Language.ToUpper(CultureInfo.InvariantCulture);
                if (Enum.TryParse(langStr, out LanguageCode language))
                {
                    song.GeniusMetaData.Language = language;
                }
            }

            // Set featured artists
            song.FeaturedArtists = songDetails.FeaturedArtists
                .Select(a => a.Name)
                .Where(name => !string.IsNullOrEmpty(name))
                .ToList();

            // Handle album
            if (songDetails.Album != null && !string.IsNullOrEmpty(songDetails.Album.Name))
            {
                Album? existingAlbum = await _albumRepo.GetAlbumAsync(songDetails.Album.Name, song.Artist);
                if (existingAlbum == null)
                {
                    existingAlbum = new Album
                    {
                        Name = songDetails.Album.Name,
                        Artist = song.Artist,
                        IsSingle = false
                    };
                    await _albumRepo.AddAsync(existingAlbum);
                }

                if (!song.Albums.Any(a => a.Name == songDetails.Album.Name))
                {
                    song.Albums.Add(existingAlbum);
                }
            }

            // Set release date
            if (!string.IsNullOrEmpty(songDetails.ReleaseDate))
            {
                if (DateOnly.TryParse(songDetails.ReleaseDate, out DateOnly releaseDate))
                {
                    song.ReleaseDate = releaseDate;
                }
            }

            return song;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error enriching song details from Genius API");
            return song;
        }
    }

    public async Task<List<GeniusHit>?> GetGeniusSearchResultsAsync(string searchQuery)
    {
        if (string.IsNullOrWhiteSpace(searchQuery))
        {
            return null;
        }
        string requestUrl = $"/search?q={Uri.EscapeDataString(searchQuery)}";
        try
        {
            _logger.LogDebug("Sending request to: {Url}", requestUrl);
            HttpResponseMessage response = await _httpClient.GetAsync(requestUrl);

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogError("Genius API error: {StatusCode}", response.StatusCode);
                return null;
            }

            string responseContent = await response.Content.ReadAsStringAsync();

            GeniusSearchResponse? searchResponse = JsonSerializer.Deserialize<GeniusSearchResponse>(responseContent, _jsonOptions);

            return searchResponse?.Response?.Hits;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "GetGeniusSearchResult: Error fetching songs from Genius API");
            return null;
        }
    }
}
