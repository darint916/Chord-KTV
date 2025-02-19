using ChordKTV.Data.Api.SongData;
using ChordKTV.Dtos;
using ChordKTV.Dtos.TranslationGptApi;
using ChordKTV.Models.SongData;
using ChordKTV.Services.Api;

namespace ChordKTV.Services.Service;

public class FullSongService : IFullSongService
{
    private readonly ILrcService _lrcService;
    private readonly IGeniusService _geniusService;
    private readonly ISongRepo _songRepo;
    private readonly IChatGptService _chatGptService;
    private readonly ILogger<FullSongService> _logger;
    public FullSongService(ILrcService lrcService, IGeniusService geniusService, ISongRepo songRepo, IChatGptService chatGptService, ILogger<FullSongService> logger)
    {
        _lrcService = lrcService;
        _geniusService = geniusService;
        _songRepo = songRepo;
        _chatGptService = chatGptService;
        _logger = logger;
    }

    public async Task<Song?> GetFullSongAsync(string? title, string? artist, TimeSpan? duration, string? lyrics, string? youtubeUrl)
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
        //TODO find alternative ways to search if not on genius? search lrc as backup? need to consider creating db entry if so
        Song? song = await _geniusService.GetSongByArtistTitleAsync(title, artist, null) ?? throw new InvalidOperationException("Song not found on Genius");

        //Gets lyrics from lrc service if not already present
        LrcLyricsDto? lyricsDto = null;
        if (string.IsNullOrWhiteSpace(song.LrcLyrics))
        {
            song.Duration ??= duration;
            float? songDuration = (float?)song.Duration?.TotalSeconds;

            lyricsDto = await _lrcService.GetLrcLibLyricsAsync(song.Title, song.Artist, null, null, null);

            if (lyricsDto is null || string.IsNullOrWhiteSpace(lyricsDto.SyncedLyrics))
            {

                _logger.LogWarning("Failed to get lyrics from LRC lib for '{Title}' by '{Artist}', Album:'{AlbumName}' Duration: {Duration}", song.Title, song.Artist, song.Albums.FirstOrDefault()?.Name, songDuration);
                if (lyricsDto != null)
                {
                    _logger.LogWarning("Lyrics: {Lyrics}", lyricsDto.SyncedLyrics);
                }

                //Try again with only title, put in as query string
                lyricsDto = await _lrcService.GetLrcLibLyricsAsync(song.Title, song.Artist, null, song.Title, songDuration);
            }
            song.LrcLyrics = lyricsDto.SyncedLyrics;

            //TODO: Refactor once issue #52 solved
            //TODO: ASSIGN LRC LYRIC ID TO SONG (not assigned in lrc service as of 2/11/25)
        }

        _logger.LogDebug("Got lyrics from LRC lib for '{Title}' by '{Artist}', Album:'{AlbumName}' Duration: {Duration}", song.Title, song.Artist, song.Albums.FirstOrDefault()?.Name, song.Duration);
        _logger.LogDebug("Lyrics: {Lyrics}", song.LrcLyrics);
        //check if lyrics are romanized (note that we do not check LRC Lib for romanization if db alr has synced lyrics)
        bool needRomanization = true;
        bool needTranslation = string.IsNullOrWhiteSpace(song.LrcTranslatedLyrics);
        if (string.IsNullOrWhiteSpace(song.LrcRomanizedLyrics) && !string.IsNullOrWhiteSpace(lyricsDto?.RomanizedSyncedLyrics))
        {
            song.LrcRomanizedLyrics = lyricsDto.RomanizedSyncedLyrics;
            needRomanization = false;
        }

        //Get Romanized and Translated lyrics from GPT if not already present
        if (string.IsNullOrWhiteSpace(song.LrcTranslatedLyrics) || needRomanization)
        {
            TranslationResponseDto translationDto = await _chatGptService.TranslateLyricsAsync(
                song.LrcLyrics, song.GeniusMetaData.Language, needRomanization, needTranslation);
            if (needRomanization && !string.IsNullOrWhiteSpace(translationDto.RomanizedLyrics))
            {
                song.LrcRomanizedLyrics = translationDto.RomanizedLyrics;
            }
            if (needTranslation && !string.IsNullOrWhiteSpace(translationDto.TranslatedLyrics))
            {
                song.LrcTranslatedLyrics = translationDto.TranslatedLyrics;
            }
        }

        //Add/Update youtube urls
        if (!string.IsNullOrWhiteSpace(youtubeUrl))
        {
            if (string.IsNullOrWhiteSpace(song.YoutubeUrl))
            {
                song.YoutubeUrl = youtubeUrl;
            }
            else if (!song.AlternateYoutubeUrls.Contains(youtubeUrl))
            {
                song.AlternateYoutubeUrls.Add(youtubeUrl);
            }
        }

        //Save to db, only update, assuming genius creates the resource
        //TODO: Consider abstracting song creation out of genius service or as flag
        await _songRepo.UpdateSongAsync(song);
        return song;
    }
}
