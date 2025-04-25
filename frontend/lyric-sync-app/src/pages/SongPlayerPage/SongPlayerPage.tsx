import React, { useState, useRef, useEffect, useMemo, useCallback } from 'react';
import { Container, Typography, Box, Button, Paper, TextField, Alert, IconButton, Tooltip, Skeleton, Slider, ToggleButton } from '@mui/material';
import YouTubePlayer from '../../components/YouTubePlayer/YouTubePlayer';
import LyricDisplay from '../../components/LyricDisplay/LyricDisplay';
import './SongPlayerPage.scss';
import Grid from '@mui/material/Grid2';
import Tabs from '@mui/material/Tabs';
import Tab from '@mui/material/Tab';
import { useSong } from '../../contexts/SongContext';
import { useNavigate } from 'react-router-dom';
import { v4 as uuidv4 } from 'uuid';
import { QueueItem } from '../../contexts/QueueTypes';
import QueueComponent from '../../components/QueueComponent/QueueComponent';
import { songApi } from '../../api/apiClient';
import { extractYouTubeVideoId, extractPlaylistId } from '../HomePage/HomePageHelpers';
import PlaylistAddIcon from '@mui/icons-material/PlaylistAdd';
import PlaylistPlayIcon from '@mui/icons-material/PlaylistPlay';
import SkipPreviousIcon from '@mui/icons-material/SkipPrevious';
import SkipNextIcon from '@mui/icons-material/SkipNext';

// Define the YouTubePlayer interface
interface YouTubePlayer {
  seekTo: (_seconds: number, _allowSeekAhead: boolean) => void;
  playVideo: () => void;
  pauseVideo: () => void;
  getCurrentTime: () => number;
  getDuration: () => number;
  setVolume: (_volume: number) => void;
}

const SongPlayerPage: React.FC = () => {
  const [currentTime, setCurrentTime] = useState<number>(0);
  const [isPlaying] = useState<boolean>(false);
  const playerRef = useRef<YouTubePlayer | null>(null);
  const [selectedTab, setSelectedTab] = useState(0);
  const [showQuizButton, setShowQuizButton] = useState(false);
  const navigate = useNavigate();
  const [songName, setSongName] = useState('');
  const [isLoading, setIsLoading] = useState(false);
  const [artistName, setArtistName] = useState('');
  const [lyrics, setLyrics] = useState('');
  const [error, setError] = useState('');
  const [youtubeUrl, setYoutubeUrl] = useState('');
  const [playlistUrl, setPlaylistUrl] = useState('');
  const [playlistLoading, setPlaylistLoading] = useState(false);
  const [lyricsOffset, setLyricsOffset] = useState<number>(0); // in seconds
  const {
    song,
    setQuizQuestions,
    setSong,
    queue,
    setQueue,
    currentPlayingId,
    setCurrentPlayingId
  } = useSong();

  useEffect(() => {
    // Restore current song if needed
    if (currentPlayingId && !song) {
      const savedSong = queue.find(item => item.queueId === currentPlayingId);
      if (savedSong) {
        setSong(savedSong);
      }
    }
  }, []);
  let animationFrameId: number;

  if (!song) {
    return <Typography variant="h5">Error: No song selected</Typography>;
  }

  const [instrumental, setInstrumental] = useState(false); // Track if instrumental is selected

  const currentQueueItem = useMemo(() => {
    return queue.find(item => item.queueId === currentPlayingId);
  }, [queue, currentPlayingId]);

  const handleKTVToggle = async () => {
    if (!song.id) return; // No song ID available

    try {
      if (!instrumental && !currentQueueItem?.ktvYouTubeId) {
        const response = await songApi.apiSongsSongIdVideoInstrumentalPut({
          songId: song.id
        })

        // Update the current song in the queue with the instrumental ID
        setQueue(prevQueue => prevQueue.map(item =>
          item.queueId === currentPlayingId
            ? {
              ...item,
              ktvYouTubeId: response
            }
            : item
        ));
      }

      setInstrumental(!instrumental);
    } catch (err) {
      const errorMessage = err instanceof Error ? err.message : 'Failed to get instrumental version';
      setError(errorMessage);
      setInstrumental(false); // Revert toggle if error
    }

  }

  const lrcTimestamps = useMemo(() => {
    const timestamps: number[] = [];
    if (!song.lrcLyrics) {
      return timestamps;
    };
    const timeTagRegex = /\[(\d+):(\d+)\.(\d+)\]/g;
    const timeMatches = [...song.lrcLyrics.matchAll(timeTagRegex)];
    timeMatches.forEach(match => {
      const minutes = parseInt(match[1]);
      const seconds = parseInt(match[2]);
      const centisecond = parseInt(match[3]);
      timestamps.push(minutes * 60 + seconds + (centisecond / 100));
    });
    timestamps.sort((a, b) => a - b);
    if (timestamps[0] > 0) {
      timestamps.unshift(0); // Add 0 if the first timestamp is greater than 0
    }
    return timestamps;
  }, [song.lrcLyrics]);


  const prevTimeRange = useRef({ start: Infinity, end: 0 });
  const currentLineRef = useRef(-1); // Track current line index

  const checkIfTimeLineChanged = (currentTime: number, timestamps: number[]) => {
    if (timestamps.length === 0 ||
      (currentTime >= prevTimeRange.current.start &&
        currentTime < prevTimeRange.current.end)) {
      return false;
    }

    for (let i = currentLineRef.current + 1; i < (timestamps.length + currentLineRef.current); i++) {
      i %= timestamps.length; // Wrap around if needed
      const currentTimestamp = timestamps[i];
      const nextTimestamp = (i < timestamps.length - 1) ? timestamps[i + 1] : Infinity;
      if (currentTime >= currentTimestamp && currentTime < nextTimestamp) {
        currentLineRef.current = i;
        prevTimeRange.current = { start: currentTimestamp, end: nextTimestamp };
        return true;
      }
    }
  };

  useEffect(() => {
    currentLineRef.current = -1;
    prevTimeRange.current = { start: Infinity, end: 0 };
  }, [song]); // Only runs on song change, so useEffect works

  const prefetchNextSongs = useCallback(async () => {
    if (!queue.length) { return; }

    const currentIndex = queue.findIndex(item => item.queueId === currentPlayingId);
    if (currentIndex >= 0 && currentIndex < queue.length - 1) {
      // Get next 2 songs (or just 1 if we're at the end)
      const nextItems = queue.slice(currentIndex + 1, currentIndex + 3);

      nextItems.forEach((nextItem) => {
        // Only proceed if we haven't already requested the API for this song
        if (!nextItem.apiRequested) {
          // Mark as requested immediately to prevent duplicate calls
          nextItem.apiRequested = true;

          // Call the API for the next song
          songApi.apiSongsMatchPost({
            fullSongRequestDto: {
              title: nextItem.title,
              artist: nextItem.artist,
              youTubeId: nextItem.youTubeId || '',
              lyrics: nextItem.lyrics || ''
            }
          }).then(response => {
            // Update the queue with the processed data
            if (nextItem.youTubeId) {
              response.youTubeId = nextItem.youTubeId;
            }
            setQueue(prevQueue => prevQueue.map(item =>
              item.queueId === nextItem.queueId
                ? {
                  ...item,
                  processedData: {
                    title: response.title,
                    artist: response.artist,
                    youTubeId: response.youTubeId,
                    lrcLyrics: response.lrcLyrics,
                    lrcRomanizedLyrics: response.lrcRomanizedLyrics,
                    lrcTranslatedLyrics: response.lrcTranslatedLyrics,
                    geniusMetaData: response.geniusMetaData,
                    id: response.id
                  }
                }
                : item
            ));
          }).catch(err => {
            const errorMessage = err instanceof Error ? err.message : 'Failed to process song';
            nextItem.error = errorMessage;
          });
        }
      });

    }
  }, [queue, currentPlayingId]);

  const allowedQuizLanguages = new Set(['AR', 'BG', 'BN', 'EL', 'FA', 'GU', 'HE', 'HI', 'JA', 'KO', 'RU', 'SR', 'TA', 'TE', 'TH', 'UK', 'ZH']);
  const isLanguageAllowedForQuiz = song.geniusMetaData?.language && allowedQuizLanguages.has(song.geniusMetaData.language);

  const updatePlayerTime = (playerInstance: YouTubePlayer) => {
    playerRef.current = playerInstance;
    playerInstance.playVideo(); // Autoplay
    setShowQuizButton(false);
    const duration = playerRef.current.getDuration();

    const updatePlayerTime = () => {
      if (playerRef.current) {
        const current = playerRef.current.getCurrentTime();


        if (checkIfTimeLineChanged(current, lrcTimestamps)) {
          setCurrentTime(current);
        }
        // Check if the song is 90% complete
        if (current / duration >= 0.9 && isLanguageAllowedForQuiz) {
          setShowQuizButton(true); // Show the quiz button when 90% complete
        }

        // Check if we're at the halfway point for prefetching
        if (current / duration >= 0.5) {
          prefetchNextSongs();
        }
      }
      animationFrameId = requestAnimationFrame(updatePlayerTime); //req next frame
    };
    cancelAnimationFrame(animationFrameId);
    updatePlayerTime();
  };

  useEffect(() => { //cleanup on rerender
    return () => {
      if (animationFrameId) {
        cancelAnimationFrame(animationFrameId); // Cancel the animation frame when the component unmounts  (cleanup function)
      };
    };
  }, [currentPlayingId]); // cleanup when current queue element changes

  const handleTabChange = (_event: React.SyntheticEvent, newValue: number) => {
    setSelectedTab(newValue);
  };

  const handleAddToNextAndPlay = async () => {
    await handleQueueAddition(true);
    setInstrumental(false); // Set KTV to unselected if new song
  };

  const handleAddToEnd = async () => {
    await handleQueueAddition(false);
  };

  const handleQuizRedirect = () => {
    setQuizQuestions([]);   // Clear old song quiz questions
    navigate('/quiz');
  };

  const handleQueueAddition = async (insertAfterCurrent: boolean = false) => {
    if (!songName.trim() && !artistName.trim() && !lyrics.trim() && !youtubeUrl.trim()) {
      setError('Please enter at least one field to search.');
      return;
    }

    setIsLoading(true);
    setError('');

    try {
      const youTubeId = extractYouTubeVideoId(youtubeUrl);
      const response = await songApi.apiSongsMatchPost({
        fullSongRequestDto: {
          title: songName.trim(),
          artist: artistName.trim(),
          youTubeId: youTubeId ?? '',
          lyrics: lyrics.trim()
        }
      });
      if (youTubeId) {
        response.youTubeId = youTubeId;
      }
      const newItem: QueueItem = {
        queueId: uuidv4(),
        apiRequested: true,
        title: response.title ?? '',
        artist: response.artist ?? '',
        youTubeId: response.youTubeId ?? '',
        lyrics: lyrics,
        processedData: response
      };

      if (!newItem.processedData?.lrcLyrics) {
        throw new Error('Failed to process song: no LRC lyrics found');
      }

      setQueue(prev => {
        if (insertAfterCurrent && currentPlayingId) {
          const currentIndex = prev.findIndex(item => item.queueId === currentPlayingId);
          if (currentIndex >= 0) {
            const newQueue = [...prev];
            newQueue.splice(currentIndex + 1, 0, newItem);
            return newQueue;
          }
        }
        // Default: add to end
        return [...prev, newItem];
      });

      // Auto-play if adding to next position
      if (insertAfterCurrent) {
        setCurrentPlayingId(newItem.queueId);
        setSong(newItem.processedData);
      }

      // Clear form
      setSongName('');
      setArtistName('');
      setLyrics('');
      setYoutubeUrl('');

    } catch (err) {
      const errorMessage = err instanceof Error ? err.message : 'Failed to process song';
      setError(errorMessage);
    } finally {
      setIsLoading(false);
    }
  };


  const handleKeyDown = (event: React.KeyboardEvent<HTMLDivElement>) => {
    if (event.key === 'Enter') {
      handleQueueAddition(true); // Add next and play on enter
    }
  };

  const handleQueueAdditionPlaylist = async () => {
    if (!playlistUrl.trim()) {
      setError('Please enter a YouTube playlist URL');
      return;
    }

    setPlaylistLoading(true);
    setError('');

    try {
      // Extract playlist ID from URL
      const playlistId = extractPlaylistId(playlistUrl);
      if (!playlistId) {
        throw new Error('Invalid YouTube playlist URL');
      }

      // Call API to get playlist items
      const response = await songApi.apiYoutubePlaylistsPlaylistIdGet({ playlistId });

      const videos = response.videos || [];
      if (videos.length === 0) { throw new Error('This playlist contains no videos'); }

      // Create queue items for each track
      const newItems: QueueItem[] = videos.map(video => ({
        queueId: uuidv4(),
        title: video.title || 'Unknown Track',
        artist: video.artist || 'Unknown Artist',
        youTubeId: extractYouTubeVideoId(video.url) || '',
        lyrics: '',
        apiRequested: false
      }));

      // Add to end of queue
      setQueue(prev => [...prev, ...newItems]);
      setPlaylistUrl('');

    } catch (err) {
      const errorMessage = err instanceof Error ? err.message : 'Failed to load playlist';
      setError(errorMessage);
    } finally {
      setPlaylistLoading(false);
    }
  };

  return (
    <div className="song-player-page">
      <Container maxWidth="lg" className="song-player-container">
        <Typography variant="h4" className="song-title" align="center" fontWeight="bold">
          {song.title}
        </Typography>
        <Typography variant="h6" className="song-title" align="center" fontWeight="bold">
          {song.artist}
        </Typography>
        {showQuizButton && (
          <Box mt={3} display="flex" justifyContent="center">
            <Button
              variant="contained"
              onClick={handleQuizRedirect}
              className="quiz-button"
            >
              Go to Quiz
            </Button>
          </Box>
        )}
        <Grid container className="song-player-content">
          {/* we use grid now as later plan to add additional column additions, change spacing if needed*/}
          <Grid size={6} alignContent={'center'} className='grid-parent'>
            {/* <YouTubePlayer videoId={song.youTubeId ?? ''} onReady={updatePlayerTime} /> */}
            <YouTubePlayer
              videoId={instrumental && currentQueueItem?.ktvYouTubeId ? currentQueueItem.ktvYouTubeId : song.youTubeId ?? ''}
              onReady={updatePlayerTime}
            />
            <Grid container spacing={2} className="controls-grid">
              <Grid size={1} alignContent={'center'}>
                <ToggleButton
                  value="check"
                  selected={instrumental}
                  // onChange={() => setInstrumental(!instrumental)}
                  onChange={handleKTVToggle}
                  className="ktv-toggle"
                  sx={{
                    display: 'flex',
                    alignItems: 'center',
                    '&.Mui-selected': {
                      // green with 60% opacity
                      bgcolor: 'rgba(76, 175, 80, 0.6)',
                      '&:hover': {
                        // same green a bit more transparent on hover
                        bgcolor: 'rgba(56, 173, 60, 0.5)',
                      },
                      fontWeight: 'bold',
                      color: 'white',
                    },
                    // optional padding tweak
                    margin: '0 auto',
                    // px: 2,
                    // py: 2,
                  }}
                >
                  KTV
                </ToggleButton>
              </Grid>
              <Grid size={1} alignContent={'center'}>
                <IconButton
                  className="queue-nav-button"
                  // onClick={handlePrevTrack}
                  // disabled={isFirst}
                  size="large"
                >
                  <SkipPreviousIcon />
                </IconButton>
              </Grid>
              <Grid size={1} alignContent={'center'}>
                <IconButton
                  className="queue-nav-button"
                  // onClick={handleNextTrack}
                  // disabled={isLast}
                  size="large"
                >
                  <SkipNextIcon />
                </IconButton>
              </Grid>
              <Grid size={9}>
                <Box mt={2} px={2}>
                  <Typography variant="body2" gutterBottom className='lrc-offset-text' align="center">
                    Lyrics Time Offset: {lyricsOffset > 0 ? `+${lyricsOffset.toFixed(1)}s` : `${lyricsOffset.toFixed(1)}s`}
                  </Typography>
                  <Slider
                    value={lyricsOffset}
                    onChange={(_, value) => setLyricsOffset(value as number)}
                    min={-5}
                    max={5}
                    step={0.1}
                    valueLabelDisplay="auto"
                    valueLabelFormat={(value) => `${value > 0 ? '+' : ''}${value}s`}
                    marks={[
                      { value: -5, label: '-5s' },
                      { value: 0, label: '0' },
                      { value: 5, label: '+5s' },
                    ]}
                  />
                </Box>
              </Grid>
            </Grid>
          </Grid>
          {/* Lyrics Column */}
          <Grid size={6} className='grid-parent'>
            <Box className='tabs-grid-parent'>
              <Tabs value={selectedTab} onChange={handleTabChange} aria-label="lyric-tabs" variant="fullWidth">
                <Tab label="Original Lyrics" />
                <Tab label="Romanized Lyrics" />
                <Tab label="Translated Lyrics" />
              </Tabs>
            </Box>
            <Box className='lrc-grid-parent'>
              <LyricDisplay
                rawLrcLyrics={
                  selectedTab === 0
                    ? song.lrcLyrics ?? 'Not supported'
                    : selectedTab === 1
                      ? song.lrcRomanizedLyrics ?? 'Not supported'
                      : song.lrcTranslatedLyrics ?? 'Not supported'
                }
                currentTime={currentTime + lyricsOffset}
                isPlaying={isPlaying}
              />
            </Box>
          </Grid>
        </Grid>
        {error && (
          <Box mt={4}>
            <Alert severity="error" className="error-alert-lrc">
              {error}
            </Alert>
          </Box>
        )}
        <Grid container spacing={3} className="bottom-grid">
          <Grid size={8}>
            <Paper elevation={3} className="search-section">
              <Typography variant="h5" className="section-title">
                Add a Song to the Queue
              </Typography>
              <Box display="flex" alignItems="center" gap={2}>
                <Box display="grid" gridTemplateColumns="1fr 1fr" gap={2} flexGrow={1}>
                  {isLoading ? (
                    <>
                      <Skeleton className="skeleton-input" variant="rectangular" />
                      <Skeleton className="skeleton-input" variant="rectangular" />
                      <Skeleton className="skeleton-input" variant="rectangular" />
                      <Skeleton className="skeleton-input" variant="rectangular" />
                    </>
                  ) : (
                    <>
                      <TextField
                        label="Song Name"
                        variant="filled"
                        value={songName}
                        onKeyDown={handleKeyDown}
                        onChange={(e) => setSongName(e.target.value)}
                        className="search-input"
                        fullWidth
                      />
                      <TextField
                        label="Artist Name"
                        variant="filled"
                        value={artistName}
                        onKeyDown={handleKeyDown}
                        onChange={(e) => setArtistName(e.target.value)}
                        className="search-input"
                        fullWidth
                      />
                      <TextField
                        label="Lyrics"
                        variant="filled"
                        value={lyrics}
                        onKeyDown={handleKeyDown}
                        onChange={(e) => setLyrics(e.target.value)}
                        className="search-input"
                        fullWidth
                      />
                      <TextField
                        label="YouTube URL"
                        variant="filled"
                        value={youtubeUrl}
                        onKeyDown={handleKeyDown}
                        onChange={(e) => setYoutubeUrl(e.target.value)}
                        className="search-input"
                        fullWidth
                      />
                    </>
                  )}
                </Box>
                <Box display="flex" flexDirection="column" gap={1}>
                  <Tooltip title="Add next and play">
                    <IconButton
                      color="primary"
                      onClick={handleAddToNextAndPlay}
                      disabled={isLoading}
                      className="queue-button"
                      size="large"
                    >
                      <PlaylistPlayIcon fontSize="inherit" />
                    </IconButton>
                  </Tooltip>

                  <Tooltip title="Add to end of queue">
                    <IconButton
                      color="secondary"
                      onClick={handleAddToEnd}
                      disabled={isLoading}
                      className="queue-button"
                      size="large"
                    >
                      <PlaylistAddIcon fontSize="inherit" />
                    </IconButton>
                  </Tooltip>
                </Box>
              </Box>
            </Paper>

            {/* YouTube Playlist Section */}
            <Paper elevation={3} className="playlist-section">
              <Typography variant="h5" className="section-title">
                Load a YouTube Playlist
              </Typography>
              <Box display="flex" alignItems="center" gap={2}>
                <Box flexGrow={1}>
                  {playlistLoading ? (
                    <Skeleton
                      className="skeleton-input"
                      variant="rectangular"
                      height={56}
                      width="100%"
                    />
                  ) : (
                    <TextField
                      fullWidth
                      label="Enter YouTube Playlist URL"
                      variant="filled"
                      value={playlistUrl}
                      disabled={isLoading || playlistLoading}
                      onKeyDown={handleKeyDown}
                      onChange={(e) => setPlaylistUrl(e.target.value)}
                      className="search-input"
                    />
                  )}
                </Box>
                <Box display="flex" flexDirection="column" gap={1}>
                  <Tooltip title="Add playlist to queue">
                    <IconButton
                      color="primary"
                      onClick={handleQueueAdditionPlaylist}
                      disabled={isLoading || playlistLoading}
                      className="queue-button"
                      size="large"
                    >
                      <PlaylistAddIcon fontSize="inherit" />
                    </IconButton>
                  </Tooltip>
                </Box>
              </Box>
            </Paper>
          </Grid>
          <Grid size={4}>
            <QueueComponent
              queue={queue}
              currentPlayingId={currentPlayingId}
              setQueue={setQueue}
              setCurrentPlayingId={setCurrentPlayingId}
              setInstrumental={setInstrumental}
            />
          </Grid>
        </Grid>
        {/* Search Section */}
      </Container>
    </div>
  );
};

export default SongPlayerPage;
