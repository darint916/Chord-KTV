using AutoMapper;
using ChordKTV.Data.Api.SongData;
using ChordKTV.Dtos;
using ChordKTV.Dtos.FullSong;
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
    private readonly IMapper _mapper;

    public FullSongService(IMapper mapper, ILrcService lrcService, IGeniusService geniusService, ISongRepo songRepo, IChatGptService chatGptService, ILogger<FullSongService> logger, IYouTubeClientService youTubeClient)
    {
        _youTubeClientService = youTubeClient;
        _lrcService = lrcService;
        _geniusService = geniusService;
        _songRepo = songRepo;
        _chatGptService = chatGptService;
        _logger = logger;
        _mapper = mapper;
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
    public async Task<FullSongResponseDto?> GetFullSongAsync(string? title, string? artist, string? album, TimeSpan? duration, string? lyrics, string? youtubeId)
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
                _logger.LogError("GetVideosDetailsAsync: Youtube video not found for id: {YoutubeId}", youtubeId);
            }
            else
            {
                videoDetails = videoDict[youtubeId];
                if (string.IsNullOrWhiteSpace(title))
                {
                    title = videoDetails.Title;
                }
                if (string.IsNullOrWhiteSpace(artist))
                {
                    artist = videoDetails.ChannelTitle;
                }
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
                        if (song is not null || (string.IsNullOrWhiteSpace(title) && string.IsNullOrWhiteSpace(artist)))
                        {
                            title = candidate.Title;
                            artist = candidate.Artist;
                            break;
                        }
                    }
                }
            }
        }
        else if (!string.IsNullOrWhiteSpace(lyrics) && string.IsNullOrWhiteSpace(title) && string.IsNullOrWhiteSpace(artist))
        {
            //since there is a high chance people search with romanization, which returns like genius romanization as artist (basically unclean title), use candidate cleaning like with youtube
            candidateSongInfoList = await _chatGptService.GetCandidateSongInfosAsync(song.Title, song.Artist);
            foreach (CandidateSongInfo candidate in candidateSongInfoList.Candidates)
            {
                song = await _geniusService.GetSongByArtistTitleAsync(candidate.Title, candidate.Artist, lyrics);
                if (song is not null || (string.IsNullOrWhiteSpace(title) && string.IsNullOrWhiteSpace(artist)))
                {
                    title = candidate.Title;
                    artist = candidate.Artist;
                    break;
                }
            }
        }

        //Gets lyrics from lrc service if not already present
        LrcLyricsDto? lrcLyricsDto = null;
        if (song is not null && string.IsNullOrWhiteSpace(song.LrcLyrics))
        {
            song.Duration ??= duration;
            float? songDuration = (float?)song.Duration?.TotalSeconds;
            lrcLyricsDto = await _lrcService.GetAllLrcLibLyricsAsync(song.Title, song.Artist, null, songDuration);
            if (lrcLyricsDto is null || string.IsNullOrWhiteSpace(lrcLyricsDto.SyncedLyrics))
            {
                _logger.LogWarning("Failed to get lyrics from LRC lib for '{Title}' by '{Artist}', Album:'{AlbumName}' Duration: {Duration}", song.Title, song.Artist, song.Albums.FirstOrDefault()?.Name, songDuration);
            }
            else
            {
                song.LrcId = lrcLyricsDto.Id;
                song.LrcLyrics = lrcLyricsDto.SyncedLyrics;
                if (lrcLyricsDto.Duration != null)
                {
                    //overwrite duration if we have a better one
                    song.Duration = TimeSpan.FromSeconds(lrcLyricsDto.Duration.Value);
                }
            }
        }

        //not found in genius
        bool songCreate = false;
        if (song is null || string.IsNullOrWhiteSpace(song.LrcLyrics))
        {
            lrcLyricsDto = await _lrcService.GetAllLrcLibLyricsAsync(title, artist, null, (float?)duration?.TotalSeconds);
            if (lrcLyricsDto is null || string.IsNullOrWhiteSpace(lrcLyricsDto.SyncedLyrics)) //not found anywhere
            {
                if (videoDetails is not null) //genius title artist failed but youtube details are there to try again
                {
                    _logger.LogWarning("2nd attempt Failed to get lyrics from LRC lib for '{Title}' by '{Artist}', Duration: {Duration}: attempting candidate gpt list", title, artist, duration);
                    candidateSongInfoList ??= await _chatGptService.GetCandidateSongInfosAsync(videoDetails.Title, videoDetails.ChannelTitle);
                    foreach (CandidateSongInfo candidate in candidateSongInfoList.Candidates)
                    {
                        _logger.LogInformation("Testing Candidate for LRC Lib: '{Title}' by '{Artist}'", candidate.Title, candidate.Artist);
                        lrcLyricsDto = await _lrcService.GetAllLrcLibLyricsAsync(candidate.Title, candidate.Artist, null, (float?)duration?.TotalSeconds);
                        if (lrcLyricsDto is not null && !string.IsNullOrWhiteSpace(lrcLyricsDto.SyncedLyrics))
                        {
                            title = candidate.Title;
                            artist = candidate.Artist;
                            break;
                        }
                    }
                }
                if (lrcLyricsDto is null || string.IsNullOrWhiteSpace(lrcLyricsDto.SyncedLyrics)) //recheck if we still dont find it with candidate list
                {
                    return _mapper.Map<FullSongResponseDto>(song); //return empty song
                }
            }

            if (song is not null) // we update if we found in genius, but had to query with user params in lrc
            {
                //covers the issue if genius gets a bad match
                song.Title = lrcLyricsDto.TrackName ?? title ?? song.Title;
                song.Artist = lrcLyricsDto.ArtistName ?? artist ?? song.Artist;
                song.LrcLyrics = lrcLyricsDto.SyncedLyrics;
                // Add new alternates from LRC search
                if (lrcLyricsDto.AlternateTitles?.Count > 0)
                {
                    foreach (string altTitle in lrcLyricsDto.AlternateTitles)
                    {
                        if (!song.AlternateTitles.Any(title => string.Equals(title, altTitle, StringComparison.OrdinalIgnoreCase)))
                        {
                            song.AlternateTitles.Add(altTitle);
                        }
                    }
                }
                if (lrcLyricsDto.AlternateArtists?.Count > 0)
                {
                    foreach (string altArtist in lrcLyricsDto.AlternateArtists)
                    {
                        if (!song.FeaturedArtists.Any(artist => string.Equals(artist, altArtist, StringComparison.OrdinalIgnoreCase)))
                        {
                            song.FeaturedArtists.Add(altArtist);
                        }
                    }
                }
            }
            else //create if we dont find in genius at all
            {

                song = new Song
                {
                    Title = lrcLyricsDto.TrackName ?? title ?? "Unknown",
                    Artist = lrcLyricsDto.ArtistName ?? artist ?? "Unknown",
                    Duration = lrcLyricsDto.Duration != null ? TimeSpan.FromSeconds(lrcLyricsDto.Duration.Value) : duration,
                    LrcLyrics = lrcLyricsDto.SyncedLyrics,
                    PlainLyrics = lrcLyricsDto.PlainLyrics,
                    LrcId = lrcLyricsDto.Id,
                    RomLrcId = lrcLyricsDto.RomanizedId,
                    LrcRomanizedLyrics = lrcLyricsDto.RomanizedSyncedLyrics,
                    AlternateTitles = lrcLyricsDto.AlternateTitles,
                    FeaturedArtists = lrcLyricsDto.AlternateArtists,
                    GeniusMetaData = new GeniusMetaData { }
                };
                songCreate = true;
            }
        }

        //Now that we have the song go through LRC + Genius, can check the DB if it matches any of the cached data based on id before we waste time processing through LLM
        if (!songCreate)
        {
            Song? dbSong = null;
            if (song.LrcId != null)
            {
                dbSong = await _songRepo.GetSongByLrcIdAsync(song.LrcId.Value);
            }
            if (dbSong is null && song.RomLrcId != null)
            {
                dbSong = await _songRepo.GetSongByRomanizedLrcIdAsync(song.RomLrcId.Value);
            }
            if (dbSong is null && song.GeniusMetaData.GeniusId != 0)
            {
                dbSong = await _songRepo.GetSongByGeniusIdAsync(song.GeniusMetaData.GeniusId);
            }
            if (dbSong is not null)
            {
                _logger.LogInformation("Found song in db with same LRC/Genius ID, skipping LLM processing");
                return _mapper.Map<FullSongResponseDto>(dbSong);
            }
        }

        // check if lyrics are translated, don't need to translate/romanize if alr english
        // Genius LangCode could be wrong, so we explicitly check romanization too.
        if (song.GeniusMetaData.Language.Equals(LanguageCode.EN) && LanguageUtils.IsRomanizedText(song.LrcLyrics))
        {
            song.LrcTranslatedLyrics ??= lrcLyricsDto?.SyncedLyrics;
            song.LrcRomanizedLyrics ??= lrcLyricsDto?.SyncedLyrics;
        }
        else if (song.GeniusMetaData.Language.Equals(LanguageCode.EN))
        {
            _logger.LogError("Genius Language code is English but lyrics are not romanized as expected, for song: '{Title}' by '{Artist}'", song.Title, song.Artist);
        }
        song.LrcRomanizedLyrics ??= lrcLyricsDto?.RomanizedSyncedLyrics;

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
        if (lrcLyricsDto != null)
        {
            if (!string.IsNullOrWhiteSpace(lrcLyricsDto.TrackName) && !song.AlternateTitles.Any(alt => alt.Equals(lrcLyricsDto.TrackName, StringComparison.OrdinalIgnoreCase)))
            {
                song.AlternateTitles.Add(lrcLyricsDto.TrackName);
            }
            if (!string.IsNullOrWhiteSpace(lrcLyricsDto.ArtistName) && !song.FeaturedArtists.Any(artist => artist.Equals(lrcLyricsDto.ArtistName, StringComparison.OrdinalIgnoreCase)))
            {
                song.FeaturedArtists.Add(lrcLyricsDto.ArtistName);
            }
            if (lrcLyricsDto.Id != 0 && song.LrcId != lrcLyricsDto.Id)
            {
                song.LrcId = lrcLyricsDto.Id; //assume new lyrics found??
            }
            if (lrcLyricsDto.RomanizedId != 0 && song.RomLrcId != lrcLyricsDto.RomanizedId)
            {
                song.RomLrcId ??= lrcLyricsDto.RomanizedId; //assume new lyrics found??
            }
            if (lrcLyricsDto.PlainLyrics is not null && string.IsNullOrWhiteSpace(song.PlainLyrics))
            {
                song.PlainLyrics = lrcLyricsDto.PlainLyrics;
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

        await _songRepo.AddSongAsync(song);
        FullSongResponseDto? response = _mapper.Map<FullSongResponseDto>(song);
        if (lrcLyricsDto != null) //add LRC
        {
            response.TitleMatchScores = lrcLyricsDto.TitleMatchScores;
            response.ArtistMatchScores = lrcLyricsDto.ArtistMatchScores;
        }
        return response;
    }
}
