using ChordKTV.Data.Api.SongData;
using ChordKTV.Dtos;
using ChordKTV.Dtos.OpenAI;
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

    //**
    // Flow of entire get full song async
    // If youtube ID supplied, we fill in missing title artist dur from youtube if those are null
    // Call genius service with title artist lyrics, if that fails and youtube details are present, we try with raw youtube details
    //     - if raw youtube details fail, we use GPT to get candidate song info and attempt with all candidates
    // If Genius did find song, call LRC Service with genius parameters if lyrics are missing in the song (since genius tries to get from db)
    // If Genius didn't find the song, try searching LRC with our user input params
    //     - if this LRC Search fails, we try with GPT generated candidate list if youtube details are present
    // If LRC search still fails with candidate list, we return the song as is
    // If LRC search succeeds, we update the song with the new lyrics and other information
    // If lyrics are not in English, we check if romanized and translated lyrics are present, if not we call GPT to get them
    // If youtube id is present, we add it to the song, if not we search for a youtube link with the song details
    // If lyrics are found from LRC, we add the track name and artist name to the song if not already present
    // If title and artist are present, we add them to the song if not already present
    // If song is created, we add it to the db, if not we update it
    //**
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

        CandidateSongInfoListResponse? candidateSongInfoList = null;

        //genius service
        Song? song = await _geniusService.GetSongByArtistTitleAsync(title, artist, lyrics);
        if (song is null)
        {
            _logger.LogWarning("Song not found on Genius");
            if (videoDetails is not null)
            {
                //first test with raw video details
                song = await _geniusService.GetSongByArtistTitleAsync(videoDetails.Title, videoDetails.ChannelTitle, lyrics);
                if (song is not null)
                {
                    title = videoDetails.Title;
                    artist = videoDetails.ChannelTitle;
                }
                else
                { //try pulling out better video details
                    _logger.LogInformation("Attempting to get song from youtube video details through GPT Parsing, as missing in genius");
                    candidateSongInfoList = await _chatGptService.GetCandidateSongInfosAsync(videoDetails.Title, videoDetails.ChannelTitle);
                    foreach (CandidateSongInfo candidate in candidateSongInfoList.Candidates)
                    {
                        song = await _geniusService.GetSongByArtistTitleAsync(candidate.Title, candidate.Artist, lyrics);
                        if (song is not null)
                        {
                            title = candidate.Title;
                            artist = candidate.Artist;
                            break;
                        }
                    }
                }
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
                _logger.LogWarning("2nd attempt Failed to get lyrics from LRC lib for '{Title}' by '{Artist}', Duration: {Duration}: attempting candidate gpt list", title, artist, duration);
                if (candidateSongInfoList is null && videoDetails is not null) //genius title artist failed but youtube details are there to try again
                {
                    candidateSongInfoList = await _chatGptService.GetCandidateSongInfosAsync(videoDetails.Title, videoDetails.ChannelTitle);
                    foreach (CandidateSongInfo candidate in candidateSongInfoList.Candidates)
                    {
                        lyricsDto = await _lrcService.GetAllLrcLibLyricsAsync(candidate.Title, candidate.Artist, null, (float?)duration?.TotalSeconds);
                        if (lyricsDto is not null && !string.IsNullOrWhiteSpace(lyricsDto.SyncedLyrics))
                        {
                            title = candidate.Title;
                            artist = candidate.Artist;
                            break;
                        }
                    }
                }
                if (lyricsDto is null || string.IsNullOrWhiteSpace(lyricsDto.SyncedLyrics)) //recheck if we still dont find it with candidate list
                {
                    return song;
                }
            }

            if (song is not null) // we update if we found in genius, but had to query with user params in lrc
            {
                //covers the issue if genius gets a bad match
                song.Title = lyricsDto.TrackName ?? title ?? song.Title;
                song.Artist = lyricsDto.ArtistName ?? artist ?? song.Artist;
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

        // check if lyrics are translated, don't need to translate/romanize if alr english
        if (song.GeniusMetaData.Language.Equals(LanguageCode.EN))
        {
            song.LrcTranslatedLyrics ??= lyricsDto?.SyncedLyrics;
            song.LrcRomanizedLyrics ??= lyricsDto?.SyncedLyrics;
        }
        song.LrcRomanizedLyrics ??= lyricsDto?.RomanizedSyncedLyrics;

        //check if lyrics are romanized (note that we do not check LRC Lib for romanization if db alr has synced lyrics)
        bool needTranslation = string.IsNullOrWhiteSpace(song.LrcTranslatedLyrics);
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
            if (!string.IsNullOrWhiteSpace(lyricsDto.TrackName) && !song.AlternateTitles.Any(alt => alt.Equals(lyricsDto.TrackName, StringComparison.OrdinalIgnoreCase)))
            {
                song.AlternateTitles.Add(lyricsDto.TrackName);
            }
            if (!string.IsNullOrWhiteSpace(lyricsDto.ArtistName) && !song.FeaturedArtists.Any(artist => artist.Equals(lyricsDto.ArtistName, StringComparison.OrdinalIgnoreCase)))
            {
                song.FeaturedArtists.Add(lyricsDto.ArtistName);
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
        if (!string.IsNullOrWhiteSpace(title) && !song.AlternateTitles.Any(alt => alt.Equals(title, StringComparison.OrdinalIgnoreCase)))
        {
            song.AlternateTitles.Add(title);
        }
        if (!string.IsNullOrWhiteSpace(artist) && !song.FeaturedArtists.Any(alt => alt.Equals(artist, StringComparison.OrdinalIgnoreCase)))
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
