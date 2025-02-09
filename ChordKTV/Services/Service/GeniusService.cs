using System.Net.Http.Headers;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Text.Json;
using ChordKTV.Services.Api;
using ChordKTV.Data.Api.SongData;
using ChordKTV.Models.SongData;
using ChordKTV.Dtos;
using System.Globalization;
using ChordKTV.Models.GeniusApi;

namespace ChordKTV.Services.Service;

public class GeniusService : IGeniusService
{
    private readonly HttpClient _httpClient;
    private readonly ISongRepo _songRepo;
    private readonly IAlbumRepo _albumRepo;
    private readonly ILogger<GeniusService> _logger;
    private readonly string _accessToken;
    private const string BaseUrl = "https://api.genius.com";

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

    public async Task<Song?> GetSongByArtistTitleAsync(string title, string? artist, bool forceRefresh = false)
    {
        // Declare existingSong outside the if block to maintain scope
        Song? existingSong = null;
        
        // Check cache first unless force refresh is requested
        if (!forceRefresh)
        {
            existingSong = artist != null
                ? await _songRepo.GetSongAsync(title, artist)
                : await _songRepo.GetSongAsync(title);

            // If we have a fully enriched song in cache, return it 
            if (existingSong != null && 
                existingSong.GeniusMetaData.GeniusId != 0)
            {
                _logger.LogDebug("Using cached song from database: {Title} by {Artist}", title, artist);
                return existingSong;
            }
        }

        // Continue with Genius API call
        string query = artist != null ? $"{title} {artist}" : title;
        string requestUrl = $"/search?q={Uri.EscapeDataString(query)}";
        
        try 
        {
            HttpResponseMessage response = await _httpClient.GetAsync(requestUrl);
            string jsonResponse = await response.Content.ReadAsStringAsync();
            
            if (!response.IsSuccessStatusCode)
            {
                _logger.LogError("Genius API error: {StatusCode} - {Response}", response.StatusCode, jsonResponse);
                return null;
            }

            _logger.LogDebug("Genius API Response: {Response}", jsonResponse);
            var searchResponse = JsonSerializer.Deserialize<GeniusSearchResponse>(jsonResponse,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            if (searchResponse?.Meta.Status != 200)
            {
                _logger.LogError("Genius API returned non-200 status");
                return null;
            }

            if (!searchResponse.Response.Hits.Any())
            {
                _logger.LogDebug("No results found for query: {Query}", query);
                return null;
            }

            var result = searchResponse.Response.Hits.First().Result;
            Song newSong = await MapGeniusResultToSongAsync(result);
            
            // Enrich the song with additional details
            newSong = await EnrichSongDetailsAsync(newSong);

            // If we got this far, store the fully enriched song
            if (newSong.GeniusMetaData.GeniusId != 0)
            {
                if (existingSong != null)
                {
                    // Update existing record
                    existingSong.GeniusMetaData = newSong.GeniusMetaData;
                    existingSong.PlainLyrics = newSong.PlainLyrics;
                    await _songRepo.UpdateAsync(existingSong);
                }
                else
                {
                    // Add new record
                    await _songRepo.AddAsync(newSong);
                }
                await _songRepo.SaveChangesAsync();
            }

            return newSong;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching song from Genius API");
            return null;
        }
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

    private async Task<Song> MapGeniusResultToSongAsync(GeniusResult result)
    {
        // First check if GeniusMetaData already exists
        var existingMetaData = await _songRepo.GetGeniusMetaDataAsync(result.Id);
        
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
            Album? existingAlbum = await _albumRepo.GetAlbumAsync(result.Album.Name, result.PrimaryArtistNames);
            if (existingAlbum != null)
            {
                song.Albums.Add(existingAlbum);
            }
            else
            {
                song.Albums.Add(new Album
                {
                    Name = result.Album.Name,
                    Artist = result.PrimaryArtistNames
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

            string jsonResponse = await response.Content.ReadAsStringAsync();
            var songResponse = JsonSerializer.Deserialize<GeniusSongResponse>(jsonResponse,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            if (songResponse?.Meta.Status != 200)
            {
                return song;
            }

            var songDetails = songResponse.Response.Song;
            
            // Set language if available
            if (!string.IsNullOrEmpty(songDetails.Language))
            {
                string langStr = songDetails.Language.ToUpper(CultureInfo.InvariantCulture);
                if (Enum.TryParse<LanguageCode>(langStr, out LanguageCode language))
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
