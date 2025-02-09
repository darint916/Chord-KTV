using System.Net.Http.Headers;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Text.Json;
using ChordKTV.Services.Api;
using ChordKTV.Data.Api.SongData;
using ChordKTV.Models.SongData;
using ChordKTV.Dtos;
using System.Globalization;

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
            using JsonDocument doc = JsonDocument.Parse(jsonResponse);
            JsonElement root = doc.RootElement;
            
            if (root.GetProperty("meta").GetProperty("status").GetInt32() != 200)
            {
                _logger.LogError("Genius API returned non-200 status");
                return null;
            }
            
            JsonElement hits = root.GetProperty("response").GetProperty("hits");
            if (!hits.EnumerateArray().Any())
            {
                _logger.LogDebug("No results found for query: {Query}", query);
                return null;
            }
            
            JsonElement result = hits.EnumerateArray().First().GetProperty("result");
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

    private async Task<Song> MapGeniusResultToSongAsync(JsonElement result)
    {
        int geniusId = result.GetProperty("id").GetInt32();
        
        // First check if GeniusMetaData already exists
        var existingMetaData = await _songRepo.GetGeniusMetaDataAsync(geniusId);
        
        GeniusMetaData metaData = existingMetaData ?? new GeniusMetaData
        {
            GeniusId = geniusId,
            HeaderImageUrl = result.GetProperty("header_image_url").GetString(),
            SongImageUrl = result.GetProperty("song_art_image_url").GetString()
        };

        string artistName = result.GetProperty("primary_artist_names").GetString() ?? string.Empty;
        
        // More robust album handling with optional chaining
        string albumName = string.Empty;
        if (result.TryGetProperty("album", out JsonElement albumElement) && 
            albumElement.ValueKind != JsonValueKind.Null)
        {
            albumName = albumElement.TryGetProperty("name", out JsonElement nameElement) ? 
                nameElement.GetString() ?? string.Empty : string.Empty;
        }

        // Check if album exists
        Album? existingAlbum = null;
        if (!string.IsNullOrEmpty(albumName))
        {
            existingAlbum = await _albumRepo.GetAlbumAsync(albumName, artistName);
        }

        // Create song object with existing or new metadata
        Song song = new()
        {
            Name = result.GetProperty("title").GetString() ?? string.Empty,
            PrimaryArtist = artistName,
            GeniusMetaData = metaData
        };

        // Add to existing album or create new one
        if (existingAlbum != null)
        {
            song.Albums.Add(existingAlbum);
        }
        else if (!string.IsNullOrEmpty(albumName))
        {
            song.Albums.Add(new Album
            {
                Name = albumName,
                Artist = artistName
            });
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
            using JsonDocument doc = JsonDocument.Parse(jsonResponse);
            JsonElement root = doc.RootElement;

            if (root.GetProperty("meta").GetProperty("status").GetInt32() != 200)
            {
                return song;
            }

            JsonElement songDetails = root.GetProperty("response").GetProperty("song");
            
            // Parse language
            if (songDetails.TryGetProperty("language", out JsonElement langElement) && 
                !string.IsNullOrEmpty(langElement.GetString()))
            {
                string langStr = langElement.GetString()!.ToUpper(CultureInfo.InvariantCulture);
                if (Enum.TryParse<LanguageCode>(langStr, out LanguageCode language))
                {
                    song.GeniusMetaData.Language = language;
                }
            }

            // Parse featured artists
            if (songDetails.TryGetProperty("featured_artists", out JsonElement featuredArtists))
            {
                song.FeaturedArtists = featuredArtists
                    .EnumerateArray()
                    .Select(a => a.GetProperty("name").GetString() ?? string.Empty)
                    .Where(name => !string.IsNullOrEmpty(name))
                    .ToList();
            }

            // Parse album details
            if (songDetails.TryGetProperty("album", out JsonElement albumElement) && 
                albumElement.ValueKind != JsonValueKind.Null)
            {
                string albumName = albumElement.GetProperty("name").GetString() ?? string.Empty;
                if (!string.IsNullOrEmpty(albumName))
                {
                    Album? existingAlbum = await _albumRepo.GetAlbumAsync(albumName, song.PrimaryArtist);
                    if (existingAlbum == null)
                    {
                        existingAlbum = new Album
                        {
                            Name = albumName,
                            Artist = song.PrimaryArtist,
                            IsSingle = false // You might want to determine this based on album type if available
                        };
                        await _albumRepo.AddAsync(existingAlbum);
                    }
                    
                    if (!song.Albums.Any(a => a.Name == albumName))
                    {
                        song.Albums.Add(existingAlbum);
                    }
                }
            }

            // Parse release date
            if (songDetails.TryGetProperty("release_date", out JsonElement releaseDateElement) && 
                !string.IsNullOrEmpty(releaseDateElement.GetString()))
            {
                if (DateOnly.TryParse(releaseDateElement.GetString(), out DateOnly releaseDate))
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
