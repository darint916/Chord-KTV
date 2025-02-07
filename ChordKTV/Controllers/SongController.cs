namespace ChordKTV.Controllers;

using System;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;
using ChordKTV.Services.Api;
using ChordKTV.Dtos;
using ChordKTV.Data.Api.SongData;
using ChordKTV.Models.SongData;
using ChordKTV.Utils.Extensions;
using Microsoft.Extensions.Logging;

[ApiController]
[Route("api")]
public class SongController : Controller
{
    private readonly ISongRepo _songRepo;
    private readonly IYouTubeClientService _youTubeService;
    private readonly ILrcService _lrcService;
    private readonly IGeniusService _geniusService;
    private readonly IAlbumRepo _albumRepo;
    private readonly ILogger<SongController> _logger;

    public SongController(/*IGeniusRepo geniusRepo,*/ IYouTubeClientService youTubeService, ILrcService lrcService, ISongRepo songRepo, IGeniusService geniusService, IAlbumRepo albumRepo, ILogger<SongController> logger)
    {
        // _geniusRepo = geniusRepo;
        _songRepo = songRepo;
        _youTubeService = youTubeService;
        _lrcService = lrcService;
        _geniusService = geniusService;
        _albumRepo = albumRepo;
        _logger = logger;
    }

    // [HttpGet("genius/{song:string}")]

    // public async Task<IActionResult> GetSongByArtistTitle(string title, string? artist)
    // {
    //     //var song = await _geniusRepo.GetSongByArtistTitle(title, artist);
    //     //return Ok(song);
    // }

    [HttpGet("youtube/playlist/{playlistId}")]
    public async Task<IActionResult> GetYouTubePlaylist(string playlistId)
    {
        PlaylistDetailsDto? result = await _youTubeService.GetPlaylistDetailsAsync(playlistId);
        if (result == null)
        {
            return StatusCode(500, new { message = "Server error: YouTube API key is missing." });
        }

        return Ok(result);
    }

    [HttpGet("lrclib/search")]
    public async Task<IActionResult> GetLrcLibLyrics([FromQuery, Required] string title, [FromQuery, Required] string artist,
                                    [FromQuery] string? albumName, [FromQuery] float? duration)
    {
        try
        {
            string? lyrics = await _lrcService.GetLrcLibLyricsAsync(title, artist, albumName, duration);

            if (lyrics == null)
            {
                return NotFound(new { message = "Lyrics not found for the specified track." });
            }

            return Ok(lyrics);
        }
        catch (HttpRequestException ex) // Handles HTTP request errors
        {
            return StatusCode(503, new { message = "Failed to fetch lyrics. Service may be unavailable.", error = ex.Message });
        }
        catch (Exception ex) // Handles unexpected errors
        {
            return StatusCode(500, new { message = "An unexpected error occurred.", error = ex.Message });
        }
    }

    [HttpGet("health")]
    public IActionResult HealthCheck()
    {
        return Ok(new { message = "Song API is healthy." });
    }

    [DevelopmentOnly]
    [HttpGet("database/song/get")]
    public async Task<IActionResult> GetSongFromDb([FromQuery, Required] string title, [FromQuery] string? artist, [FromQuery] string? albumName)
    {
        //there is no case for album name but no artist, maybe add in future or not need. This for dev testing
        Song? song = null;
        if (!string.IsNullOrEmpty(albumName) && !string.IsNullOrEmpty(artist))
        {
            song = await _songRepo.GetSongAsync(title, artist, albumName);
        }
        else if (!string.IsNullOrEmpty(artist))
        {
            song = await _songRepo.GetSongAsync(title, artist);
        }
        song ??= await _songRepo.GetSongAsync(title);
        if (song == null)
        {
            return NotFound(new { message = "Song not found in database." });
        }
        return Ok(song);
    }

    [DevelopmentOnly]
    [HttpPost("database/song/add")]
    public async Task<IActionResult> AddSongToDb([FromBody] Song song)
    {
        await _songRepo.AddAsync(song);
        return Ok();
    }

    [HttpGet("genius/search")]
    public async Task<IActionResult> GetSongByArtistTitle([FromQuery, Required] string title, [FromQuery] string? artist)
    {
        try
        {
            Song? song = await _geniusService.GetSongByArtistTitleAsync(title, artist);
            if (song == null)
            {
                return NotFound(new { message = "Song not found on Genius." });
            }

            // Enrich with additional details
            song = await _geniusService.EnrichSongDetailsAsync(song);

            // Handle albums (this part already checks for duplicates)
            if (song?.Albums != null)
            {
                foreach (Album album in song.Albums)
                {
                    Album? existingAlbum = await _albumRepo.GetAlbumAsync(album.Name, album.Artist);
                    if (existingAlbum == null)
                    {
                        await _albumRepo.AddAsync(album);
                    }
                }
            }

            // Instead of always adding the song, check first
            Song? existingSong = await _songRepo.GetSongAsync(song.Name, song.PrimaryArtist);
            if (existingSong == null)
            {
                await _songRepo.AddAsync(song);
            }

            SongDto dto = new(
                song.Name,
                song.PrimaryArtist,
                song.FeaturedArtists,
                song.Albums.Select(a => a.Name).ToList(),
                song.Genre,
                song.PlainLyrics,
                new GeniusMetaDataDto(
                    song.GeniusMetaData.GeniusId,
                    song.GeniusMetaData.HeaderImageUrl,
                    song.GeniusMetaData.SongImageUrl,
                    song.GeniusMetaData.Language
                )
            );

            return Ok(dto);
        }
        catch (HttpRequestException ex)
        {
            return StatusCode(503, new { message = "Failed to fetch from Genius API.", error = ex.Message });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "An unexpected error occurred.", error = ex.Message });
        }
    }

    [HttpPost("genius/search/batch")]
    public async Task<IActionResult> GetSongsByArtistTitle([FromBody, Required] List<VideoInfo> videos)
    {
        try
        {
            List<Song> songs = await _geniusService.GetSongsByArtistTitleAsync(videos);
            if (!songs.Any())
            {
                return NotFound(new { message = "No songs found on Genius." });
            }

            List<SongDto> dtos = new();
            foreach (Song song in songs)
            {
                // Handle albums first
                if (song?.Albums != null)
                {
                    foreach (Album album in song.Albums)
                    {
                        Album? existingAlbum = await _albumRepo.GetAlbumAsync(album.Name, album.Artist);
                        if (existingAlbum == null)
                        {
                            await _albumRepo.AddAsync(album);
                        }
                    }
                }

                // Save the song
                await _songRepo.AddAsync(song);

                // Map to DTO
                dtos.Add(new SongDto(
                    song.Name,
                    song.PrimaryArtist,
                    song.FeaturedArtists,
                    song.Albums.Select(a => a.Name).ToList(),
                    song.Genre,
                    song.PlainLyrics,
                    new GeniusMetaDataDto(
                        song.GeniusMetaData.GeniusId,
                        song.GeniusMetaData.HeaderImageUrl,
                        song.GeniusMetaData.SongImageUrl,
                        song.GeniusMetaData.Language
                    )
                ));
            }

            return Ok(dtos);
        }
        catch (HttpRequestException ex)
        {
            return StatusCode(503, new { message = "Failed to fetch from Genius API.", error = ex.Message });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "An unexpected error occurred.", error = ex.Message });
        }
    }
}
