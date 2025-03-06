using ChordKTV.Data.Api.SongData;
using ChordKTV.Dtos;
using ChordKTV.Dtos.TranslationGptApi;
using ChordKTV.Dtos.YouTubeApi;
using ChordKTV.Models.SongData;
using ChordKTV.Services.Api;
using ChordKTV.Utils;

namespace ChordKTV.Services.Service;

public class FullSongService : IFullSongService
{
    private readonly IYouTubeClientService _youTubeClientService;
    private readonly ILrcService _lrcService;
    private readonly IGeniusService _geniusService;
    private readonly ISongRepo _songRepo;
    private readonly IChatGptService _chatGptService;
    private readonly ILogger<FullSongService> _logger;
    public FullSongService(ILrcService lrcService, IGeniusService geniusService, ISongRepo songRepo, IChatGptService chatGptService, ILogger<FullSongService> logger, IYouTubeClientService youTubeClient)
    {
        _youTubeClientService = youTubeClient;
        _lrcService = lrcService;
        _geniusService = geniusService;
        _songRepo = songRepo;
        _chatGptService = chatGptService;
        _logger = logger;
    }

    //just realized we dont take album lmao
    public async Task<Song?> GetFullSongAsync(string? title, string? artist, string? album, TimeSpan? duration, string? lyrics, string? youtubeId)
    {
        if (string.IsNullOrWhiteSpace(title) && string.IsNullOrWhiteSpace(lyrics) && string.IsNullOrWhiteSpace(youtubeId))
        {
            throw new ArgumentException("GetFullSongAsync: Title or lyrics or youtubeid must be provided");
        }

        //if youtube id first supplied, we just use that to search > user input if possible
        VideoDetails? videoDetails = null;
        if (!string.IsNullOrWhiteSpace(youtubeId))
        {
            Dictionary<string, VideoDetails> videoDict = await _youTubeClientService.GetVideosDetailsAsync([youtubeId]);
            if (videoDict.Count == 0)
            {
                _logger.LogWarning("GetVideosDetailsAsync: Youtube video not found for id: {YoutubeId}", youtubeId);
            }
            else
            {
                videoDetails = videoDict[youtubeId];
                title ??= videoDetails.Title;
                artist ??= videoDetails.ChannelTitle;
                duration ??= videoDetails.Duration;
            }
        }

        //genius service
        Song? song = await _geniusService.GetSongByArtistTitleAsync(title, artist, lyrics);
        if (song is null)
        {
            _logger.LogWarning("Song not found on Genius");
            if (videoDetails is not null)
            {
                _logger.LogInformation("Attempting to get song from youtube video details, as missing in genius");
                song = await _geniusService.GetSongByArtistTitleAsync(videoDetails.Title, videoDetails.ChannelTitle, lyrics);
                title = videoDetails.Title; //overwrite title and artist since original query failed
                artist = videoDetails.ChannelTitle;
                duration = videoDetails.Duration;
            }
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
                _logger.LogWarning("2nd attempt Failed to get lyrics from LRC lib for '{Title}' by '{Artist}', Duration: {Duration}", title, artist, duration);
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
        if (!string.IsNullOrWhiteSpace(youtubeId))
        {
            if (string.IsNullOrWhiteSpace(song.YoutubeId))
            {
                song.YoutubeId = youtubeId;
            }
            else if (!song.AlternateYoutubeIds.Contains(youtubeId))
            {
                song.AlternateYoutubeIds.Add(youtubeId);
            }
        }
        else if (string.IsNullOrWhiteSpace(song.YoutubeId)) //query for a vid if none provided and non exist, expensive call
        {
            song.YoutubeId = await _youTubeClientService.SearchYoutubeVideoLinkAsync(song.Title, song.Artist, song.Albums.FirstOrDefault()?.Name, song.Duration);
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
