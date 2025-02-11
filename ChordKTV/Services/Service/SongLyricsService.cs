
namespace ChordKTV.Services.Service;

using ChordKTV.Data.Api.SongData;
using ChordKTV.Dtos;
using ChordKTV.Services.Api;

public class SongLyricsService
{
    private readonly ILrcService _lrcService;
    private readonly IYouTubeClientService _youTubeClientService;
    private readonly IGeniusService _geniusService;
    private readonly ISongRepo _songRepo;
    public SongLyricsService(ILrcService lrcService, IYouTubeClientService youTubeClientService, IGeniusService geniusService, ISongRepo songRepo)
    {
        _lrcService = lrcService;
        _youTubeClientService = youTubeClientService;
        _geniusService = geniusService;
        _songRepo = songRepo;
    }

    public async Task<LrcLyricsDto?> GetLrcLibLyricsAsync(string? title, string? artist, TimeSpan? duration, string? lyrics)
    {
        if (string.IsNullOrWhiteSpace(title) && string.IsNullOrWhiteSpace(lyrics))
        {
            throw new ArgumentException("Title or lyrics must be provided");
        }
        if (!string.IsNullOrWhiteSpace(lyrics))
        {
            throw new NotImplementedException("Lyrics search not implemented, issue #35");
        }

        //genius service
        //intellisense cant read the above null avoiding logic smh
        ArgumentNullException.ThrowIfNull(title);
        Models.SongData.Song? song = await _geniusService.GetSongByArtistTitleAsync(title, artist);
        if (song == null)
        {
            return null; //TODO find alternative ways to search if not on genius? search lrc as backup? need to consider creating db entry if so
        }

        //check if lyrics exist
        if (string.IsNullOrWhiteSpace(song.LrcLyrics))
        {
            return null;
        }

        // if (string.IsNullOrWhiteSpace(song.LrcLyrics))
        // {
        //     LrcLyricsDto lyrics = _lrcService.GetLrcLibLyricsAsync(song.Title, song.Artist, song.Albums.First() ??, duration);
        //     lyrics.
        //     return song;
        // }
        //check if lyrics are romanized
        //lrc service


        //  await _lrcService.GetLrcLibLyricsAsync(title, artist, albumName, duration);
    }

}
