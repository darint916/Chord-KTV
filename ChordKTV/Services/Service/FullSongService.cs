using ChordKTV.Data.Api.SongData;
using ChordKTV.Dtos;
using ChordKTV.Models.SongData;
using ChordKTV.Services.Api;

namespace ChordKTV.Services.Service;

public class FullSongService : IFullSongService
{
    private readonly ILrcService _lrcService;
    private readonly IGeniusService _geniusService;
    private readonly ISongRepo _songRepo;
    private readonly IChatGptService _chatGptService;
    public FullSongService(ILrcService lrcService, IGeniusService geniusService, ISongRepo songRepo, IChatGptService chatGptService)
    {
        _lrcService = lrcService;
        _geniusService = geniusService;
        _songRepo = songRepo;
        _chatGptService = chatGptService;
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
        //intellisense cant read the above null avoiding logic smh
        ArgumentNullException.ThrowIfNull(title);
        Song? song = await _geniusService.GetSongByArtistTitleAsync(title, artist);
        if (song == null)
        {
            return null; //TODO find alternative ways to search if not on genius? search lrc as backup? need to consider creating db entry if so
        }

        //Gets lyrics from lrc service if not already present
        LrcLyricsDto? lyricsDto = null;
        if (string.IsNullOrWhiteSpace(song.LrcLyrics))
        {
            song.Duration ??= duration;
            float? songDuration = (float?)song.Duration?.TotalSeconds;
            lyricsDto = await _lrcService.GetLrcLibLyricsAsync(song.Title, song.Artist, song.Albums.FirstOrDefault()?.Artist, songDuration);
            if (lyricsDto is null || string.IsNullOrWhiteSpace(lyricsDto.LrcLyrics))
            {
                return song;
            }
            song.LrcLyrics = lyricsDto.LrcLyrics;
            //TODO: ASSIGN LRC LYRICID TO SONG (not assigned in lrc service as of 2/11/25)
        }

        //check if lyrics are romanized (note that we do not check LRC Lib for romanization if db alr has synced lyrics)
        bool needRomanization = true;
        bool needTranslation = string.IsNullOrWhiteSpace(song.TranslatedLyrics);
        if (string.IsNullOrWhiteSpace(song.RomanizedLyrics) && !string.IsNullOrWhiteSpace(lyricsDto?.RomanizedSyncedLyrics))
        {
            song.RomanizedLyrics = lyricsDto.RomanizedSyncedLyrics;
            needRomanization = false;
        }

        //Get Romanized and Translated lyrics from GPT if not already present
        if (string.IsNullOrWhiteSpace(song.TranslatedLyrics) || needRomanization)
        {
            TranslationResponseDto translationDto = await _chatGptService.TranslateLyricsAsync(
                song.LrcLyrics, song.GeniusMetaData.Language, needRomanization, needTranslation);
            if (needRomanization && !string.IsNullOrWhiteSpace(translationDto.RomanizedLyrics))
            {
                song.RomanizedLyrics = translationDto.RomanizedLyrics;
            }
            if (needTranslation && !string.IsNullOrWhiteSpace(translationDto.TranslatedLyrics))
            {
                song.TranslatedLyrics = translationDto.TranslatedLyrics;
            }
        }

        //Add/Update youtube urls
        if(string.IsNullOrWhiteSpace(song.YoutubeUrl) && !string.IsNullOrWhiteSpace(youtubeUrl))
        {
            song.YoutubeUrl = youtubeUrl;
        }
        else if (!string.IsNullOrWhiteSpace(youtubeUrl) && !song.AlternateYoutubeUrls.Contains(song.YoutubeUrl))
        {
            song.AlternateYoutubeUrls.Add(youtubeUrl);
        }

        //Save to db, only update, assuming genius creates the resource
        //TODO: Consider abstracting song creation out of genius service or as flag
        await _songRepo.UpdateSongAsync(song);
        return song;
    }
}
