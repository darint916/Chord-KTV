using ChordKTV.Data.Api.SongData;
using ChordKTV.Dtos;
using ChordKTV.Dtos.OpenAI;
using ChordKTV.Dtos.TranslationGptApi;
using ChordKTV.Models.SongData;
using ChordKTV.Services.Api;
using ChordKTV.Utils;

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

    //just realized we dont take album lmao
    public async Task<Song?> GetFullSongAsync(string? title, string? artist, string? album, TimeSpan? duration, string? lyrics, string? youtubeUrl)
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
        Song? song = await _geniusService.GetSongByArtistTitleAsync(title, artist, lyrics);
        if (song is null)
        {
            _logger.LogWarning("Song not found on Genius");
        }

        //Gets lyrics from lrc service if not already present
        LrcLyricsDto? lyricsDto = null;
        if (song is not null && string.IsNullOrWhiteSpace(song.LrcLyrics))
        {
            song.Duration ??= duration;
            float? songDuration = (float?)song.Duration?.TotalSeconds;
            lyricsDto = await _lrcService.GetAllLrcLibLyricsAsync(song.Title, song.Artist, null, songDuration);
            if (lyricsDto is null || string.IsNullOrWhiteSpace(lyricsDto.SyncedLyrics))
            {
                _logger.LogWarning("Failed to get lyrics from LRC lib for '{Title}' by '{Artist}', Album:'{AlbumName}' Duration: {Duration}", song.Title, song.Artist, song.Albums.FirstOrDefault()?.Name, songDuration);
            }
            else
            {
                song.LrcId = lyricsDto.Id;
                song.LrcLyrics = lyricsDto.SyncedLyrics;
                song.Duration = TimeSpan.FromSeconds(lyricsDto.Duration); //overwrite since we overwrite lyrics above too
            }
        }

        //not found in genius
        bool songCreate = false;
        if (song is null || string.IsNullOrWhiteSpace(song.LrcLyrics))
        {
            lyricsDto = await _lrcService.GetAllLrcLibLyricsAsync(title, artist, null, (float?)duration?.TotalSeconds);
            if (lyricsDto is null || string.IsNullOrWhiteSpace(lyricsDto.SyncedLyrics)) //not found anywhere
            {
                _logger.LogWarning("2nd attempt Failed to get lyrics from LRC lib for '{Title}' by '{Artist}', Duration: {Duration}, attempting GPT extraction: ", title, artist, duration);

                //Get new title artist candidates from gpt if from youtube
                CandidateSongInfoListResponse candidateSongInfoList = await _chatGptService.GetCandidateSongInfosAsync(title, artist);

                return song;
            }

            if (song is not null) // we update if we found in genius, but had to query with user params in lrc
            {
                song.LrcLyrics = lyricsDto.SyncedLyrics;
            }
            else //create if we dont find in genius at all
            {
                song = new Song
                {
                    Title = lyricsDto.TrackName ?? title ?? "Unknown",
                    Artist = lyricsDto.ArtistName ?? artist ?? "Unknown",
                    Duration = lyricsDto.Duration > 0 ? TimeSpan.FromSeconds(lyricsDto.Duration) : duration,
                    LrcLyrics = lyricsDto.SyncedLyrics,
                    PlainLyrics = lyricsDto.PlainLyrics,
                    LrcId = lyricsDto.Id,
                    RomLrcId = lyricsDto.RomanizedId,
                    LrcRomanizedLyrics = lyricsDto.RomanizedSyncedLyrics,
                    GeniusMetaData = new GeniusMetaData { }
                };
                songCreate = true;
            }
        }

        // check if lyrics are translated, don't need to translate alr english
        if (song.GeniusMetaData.Language.Equals(LanguageCode.EN))
        {
            song.LrcRomanizedLyrics ??= lyricsDto?.RomanizedSyncedLyrics;
        }
        bool needTranslation = string.IsNullOrWhiteSpace(song.LrcTranslatedLyrics);

        //check if lyrics are romanized (note that we do not check LRC Lib for romanization if db alr has synced lyrics)
        song.LrcRomanizedLyrics ??= lyricsDto?.RomanizedSyncedLyrics; //only assigns when song rom null
        bool needRomanization = string.IsNullOrWhiteSpace(song.LrcRomanizedLyrics); //if still null, gpt rom

        //Get Romanized and Translated lyrics from GPT if not already present
        if (needTranslation || needRomanization)
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

        //Add residual information (kinda messy)
        if (lyricsDto != null)
        {
            if (lyricsDto.TrackName is not null && !song.AlternateTitles.Contains(lyricsDto.TrackName.ToLowerInvariant()) && !string.IsNullOrWhiteSpace(lyricsDto.TrackName))
            {
                song.AlternateTitles.Add(lyricsDto.TrackName.ToLowerInvariant());
            }
            if (lyricsDto.ArtistName is not null && !song.FeaturedArtists.Contains(lyricsDto.ArtistName.ToLowerInvariant()) && !string.IsNullOrWhiteSpace(lyricsDto.ArtistName))
            {

                song.FeaturedArtists.Add(lyricsDto.ArtistName.ToLowerInvariant());
            }
            if (lyricsDto.Id != 0 && song.LrcId != lyricsDto.Id)
            {
                song.LrcId = lyricsDto.Id; //assume new lyrics found??
            }
            if (lyricsDto.RomanizedId != 0 && song.RomLrcId != lyricsDto.RomanizedId)
            {
                song.RomLrcId ??= lyricsDto.RomanizedId; //assume new lyrics found??
            }
            if (lyricsDto.PlainLyrics is not null && string.IsNullOrWhiteSpace(song.PlainLyrics))
            {
                song.PlainLyrics = lyricsDto.PlainLyrics;
            }
        }
        if (title is not null && !song.AlternateTitles.Contains(title) && !string.IsNullOrWhiteSpace(title))
        {
            song.AlternateTitles.Add(title);
        }
        if (artist is not null && !song.FeaturedArtists.Contains(artist) && !string.IsNullOrWhiteSpace(artist))
        {
            if (CompareUtils.CompareArtistFuzzyScore(song.Artist, artist) > 75) //filters out youtube personal channels
            {
                song.FeaturedArtists.Add(artist);
            }
        }

        //Save to db, only update, assuming genius creates the resource
        //TODO: Consider abstracting song creation out of genius service or as flag
        if (songCreate)
        {
            await _songRepo.AddSongAsync(song);
        }
        else
        {
            await _songRepo.UpdateSongAsync(song);
        }
        return song;
    }
}
