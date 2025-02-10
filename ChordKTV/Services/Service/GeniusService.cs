using System.Net.Http.Headers;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
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

    private static bool IsFuzzyMatch(GeniusResult result, string queryTitle, string? queryArtist)
    {
        // Only match on title - we trust Genius's artist data
        int titleScore = Fuzz.Ratio(result.Title.ToLower(CultureInfo.CurrentCulture), queryTitle.ToLower(CultureInfo.CurrentCulture));
        return titleScore >= MINIMUM_FUZZY_RATIO;
    }

    /// <summary>
    /// Helper to query Genius using the given search query and compare results with the provided fuzzy criteria
    /// </summary>
    private async Task<Song?> SearchGenius(string searchQuery, string fuzzyTitle, string? fuzzyArtist)
    {
        string requestUrl = $"/search?q={Uri.EscapeDataString(searchQuery)}";
        try
        {
            HttpResponseMessage response = await _httpClient.GetAsync(requestUrl);
            if (!response.IsSuccessStatusCode)
            {
                _logger.LogError("Genius API error: {StatusCode}", response.StatusCode);
                return null;
            }

            GeniusSearchResponse? searchResponse = await response.Content.ReadFromJsonAsync<GeniusSearchResponse>(
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            if (searchResponse?.Response?.Hits == null || searchResponse.Meta.Status != 200)
            {
                return null;
            }

            // Use the provided fuzzy parameters (which can be different for the fallback scenario)
            GeniusHit? matchingHit = searchResponse.Response.Hits
                .FirstOrDefault(h => h.Result != null && IsFuzzyMatch(h.Result, fuzzyTitle, fuzzyArtist));

            if (matchingHit?.Result == null)
            {
                return null;
            }

            return await MapGeniusResultToSongAsync(matchingHit.Result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching song from Genius API");
            return null;
        }
    }

    public async Task<Song?> GetSongByArtistTitleAsync(string title, string? artist, bool forceRefresh = false)
    {
        // Instead of relying solely on the cached record,
        // we also check that the cached song has a valid (non-empty) PrimaryArtist.
        Song? existingSong = null;
        if (!forceRefresh)
        {
            existingSong = await _songRepo.GetSongAsync(title);
            if (existingSong != null && existingSong.GeniusMetaData.GeniusId != 0 && !string.IsNullOrWhiteSpace(existingSong.PrimaryArtist))
            {
                _logger.LogDebug("Using cached song from database: {Title} by {PrimaryArtist}", title, existingSong.PrimaryArtist);
                return existingSong;
            }
        }

        // Use the user input to construct the query for Genius.
        // Even if the artist is incorrect, the combined query might help Genius return the correct song.
        string primaryQuery = !string.IsNullOrEmpty(artist) ? $"{title} {artist}" : title;
        Song? result = await SearchGenius(primaryQuery, title, artist);

        // Fallback: if no result was found with the combined query and an artist was provided, try title only.
        if (result == null && !string.IsNullOrEmpty(artist))
        {
            _logger.LogDebug("No results found with artist, trying title only: {Title}", title);
            result = await SearchGenius(title, title, null);
        }

        if (result == null)
        {
            return null;
        }

        // Enrich the song details from Genius so that the result contains the authoritative PrimaryArtist.
        Song? enrichedSong = await EnrichSongDetailsAsync(result);

        // Check if enrichment failed
        if (enrichedSong == null)
        {
            return null;
        }

        // Here we update the cached record (or add a new record) with the correct data from Genius.
        if (existingSong != null)
        {
            // Update the cached entry with the correct singer name from Genius.
            existingSong.PrimaryArtist = enrichedSong.PrimaryArtist;
            existingSong.GeniusMetaData = enrichedSong.GeniusMetaData;
            existingSong.PlainLyrics = enrichedSong.PlainLyrics;
            await _songRepo.UpdateAsync(existingSong);
            return existingSong;
        }

        await _songRepo.AddAsync(enrichedSong);
        return enrichedSong;
    }

    public async Task<List<Song>> GetSongsByArtistTitleAsync(List<VideoInfo> videos, bool forceRefresh = false)
    {
        List<Song> songs = [];
        foreach (VideoInfo video in videos)
        {
            Song? song = await GetSongByArtistTitleAsync(video.Title, video.Artist, forceRefresh);
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
            Name = result.Title,
            PrimaryArtist = result.PrimaryArtistNames,
            GeniusMetaData = metaData
        };

        // Handle album if present
        if (result.Album != null && !string.IsNullOrEmpty(result.Album.Name))
        {
            Album? existingAlbum = await _albumRepo.GetAlbumAsync(result.Album.Name, song.PrimaryArtist);
            if (existingAlbum != null)
            {
                song.Albums.Add(existingAlbum);
            }
            else
            {
                song.Albums.Add(new Album
                {
                    Name = result.Album.Name,
                    Artist = song.PrimaryArtist
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
                Album? existingAlbum = await _albumRepo.GetAlbumAsync(songDetails.Album.Name, song.PrimaryArtist);
                if (existingAlbum == null)
                {
                    existingAlbum = new Album
                    {
                        Name = songDetails.Album.Name,
                        Artist = song.PrimaryArtist,
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
