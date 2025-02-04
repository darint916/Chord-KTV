namespace ChordKTV.Controllers;

using System;
using Microsoft.AspNetCore.Mvc;
using ChordKTV.Models.ApiModels;
using ChordKTV.Services.Api;

[ApiController]
[Route("api/song")]
public class SongController : Controller
{
    // private readonly IGeniusRepo _geniusRepo;
    private readonly IYouTubeService _youTubeService;

    public SongController(/*IGeniusRepo geniusRepo,*/ IYouTubeService youTubeService)
    {
        // _geniusRepo = geniusRepo;
        _youTubeService = youTubeService;
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
        PlaylistDetailsDto result = await _youTubeService.GetPlaylistDetailsAsync(playlistId);
        return Ok(result);
    }
}
