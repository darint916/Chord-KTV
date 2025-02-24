using System.Net.Http.Headers;
using System.Text.Json;
using ChordKTV.Services.Api;
using ChordKTV.Data.Api.SongData;
using ChordKTV.Models.SongData;
using ChordKTV.Dtos;
using System.Globalization;
using ChordKTV.Dtos.GeniusApi;
using FuzzySharp;

namespace ChordKTV.Services.Service;

public class GeniusService : IGeniusService
{
    private readonly HttpClient _httpClient;
    private readonly ISongRepo _songRepo;
    private readonly IAlbumRepo _albumRepo;
    private readonly ILogger<GeniusService> _logger;
    private readonly string _accessToken;
    private const string BaseUrl = "https://api.genius.com";
    private const int MINIMUM_FUZZY_RATIO = 80; // Adjust this threshold as needed

    public GeniusService(
        IConfiguration configuration,
        HttpClient httpClient,
        ISongRepo songRepo,
        IAlbumRepo albumRepo,
        ILogger<GeniusService> logger)
    {
        _accessToken = configuration["Genius:ApiKey"] ??
            throw new ArgumentNullException(nameof(configuration), "Genius API key is required");
        _httpClient = httpClient;
        _songRepo = songRepo;
        _albumRepo = albumRepo;
        _logger = logger;

        _httpClient.BaseAddress = new Uri(BaseUrl);
        _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _accessToken);
    }

    private sealed class SearchMatch
    {
        public GeniusResult Result { get; set; } = null!;
        public int TitleScore { get; set; }
        public int ArtistScore { get; set; }
        public int TotalScore => TitleScore + ArtistScore;
    }

    private static SearchMatch ScoreMatch(GeniusResult result, string queryTitle, string? queryArtist)
    {
        // Calculate title score
        string normalizedResultTitle = result.Title.ToLower(CultureInfo.CurrentCulture);
        string normalizedQueryTitle = queryTitle.ToLower(CultureInfo.CurrentCulture);

        int exactTitleScore = Fuzz.Ratio(normalizedResultTitle, normalizedQueryTitle);
        bool containsTitle = normalizedResultTitle.Contains(normalizedQueryTitle, StringComparison.OrdinalIgnoreCase);
        int titleScore = containsTitle ? Math.Max(exactTitleScore, 85) : exactTitleScore;

        // Calculate artist score only if provided (used for ranking, not filtering)
        int artistScore = 0;
        if (!string.IsNullOrWhiteSpace(queryArtist))
        {
            artistScore = Fuzz.Ratio(
                result.PrimaryArtistNames.ToLower(CultureInfo.CurrentCulture),
                queryArtist.ToLower(CultureInfo.CurrentCulture)
            );
        }

        return new SearchMatch
        {
            Result = result,
            TitleScore = titleScore,
            ArtistScore = artistScore
        };
    }

    private static bool IsFuzzyMatch(GeniusResult result, string? queryTitle, string? queryArtist)
    {
        // If no title provided (lyrics-only search), skip fuzzy title matching
        if (string.IsNullOrWhiteSpace(queryTitle))
        {
            return true; // Accept all results when searching by lyrics only
        }

        // Only match on title - we trust Genius's artist data
        int titleScore = Fuzz.Ratio(result.Title.ToLower(CultureInfo.CurrentCulture), queryTitle.ToLower(CultureInfo.CurrentCulture));
        return titleScore >= MINIMUM_FUZZY_RATIO;
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
                responseContent, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            if (searchResponse?.Response?.Hits == null || searchResponse.Meta.Status != 200)
            {
                _logger.LogWarning("Invalid response structure or status. Meta Status: {Status}",
                    searchResponse?.Meta.Status);
                return null;
            }

            _logger.LogInformation("Retrieved {Count} hits from Genius", searchResponse.Response.Hits.Count);

            // Log all results before scoring
            foreach (GeniusHit hit in searchResponse.Response.Hits)
            {
                _logger.LogDebug("Found result - Title: '{Title}', Artist: '{Artist}'",
                    hit.Result.Title, hit.Result.PrimaryArtistNames);
            }

            const int MIN_TITLE_SCORE = 50;  // Only basic title threshold to filter out completely unrelated matches

            // Simply get all matches with a reasonable title score, sorted by total score
            var matches = searchResponse.Response.Hits
                .Where(h => h.Result != null)
                .Select(h => ScoreMatch(h.Result, fuzzyTitle ?? "", fuzzyArtist))
                .Where(m => m.TitleScore >= MIN_TITLE_SCORE)
                .OrderByDescending(m => m.TotalScore)
                .ToList();

            _logger.LogInformation("Found {Count} potential matches", matches.Count);

            // Log top matches for debugging
            foreach (SearchMatch? match in matches.Take(3))
            {
                _logger.LogInformation(
                    "Match - Title: '{Title}', Artist: '{Artist}', Scores: Title={TitleScore}, Artist={ArtistScore}, Total={TotalScore}",
                    match.Result.Title, match.Result.PrimaryArtistNames, match.TitleScore, match.ArtistScore, match.TotalScore
                );
            }

            SearchMatch? bestMatch = matches.FirstOrDefault();
            if (bestMatch?.Result == null)
            {
                _logger.LogWarning("No matches found with minimum title score of {MinScore}", MIN_TITLE_SCORE);
                return null;
            }

            return await MapGeniusResultToSongAsync(bestMatch.Result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching song from Genius API");
            return null;
        }
    }

    public async Task<Song?> GetSongByArtistTitleAsync(string? title, string? artist, string? lyrics, bool forceRefresh = false)
    {
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
            _logger.LogInformation("Found matching song. Adding to cache.");
            await _songRepo.AddSongAsync(result);
            await _songRepo.SaveChangesAsync();
        }
        else
        {
            _logger.LogWarning("No matching song found after all search attempts");
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
        GeniusMetaData? existingMetaData = await _songRepo.GetGeniusMetaDataAsync(result.Id);

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

    public async Task<Song?> EnrichSongDetailsAsync(Song song)
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
}
