import React, { useState, useRef, useEffect, useMemo, useCallback } from 'react';
import { Container, Typography, Box, Button, Paper, TextField, Alert, IconButton, Tooltip, Skeleton, Slider, ToggleButton, styled } from '@mui/material';
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
import SkipPreviousIcon from '@mui/icons-material/SkipPrevious';
import SkipNextIcon from '@mui/icons-material/SkipNext';
import MuiInput from '@mui/material/Input';
import AddIcon from '@mui/icons-material/Add';


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
  const [minLyricOffset, setMinLyricOffset] = useState<number>(-1);
  const [maxLyricOffset, setMaxLyricOffset] = useState<number>(1);
  const [autoPlayEnabled, setAutoPlayEnabled] = useState(false);
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
    currentLineRef.current = -1;
    prevTimeRange.current = { start: Infinity, end: 0 };
  }, [song]); // Only runs on song change, so useEffect works

  useEffect(() => {
    // Restore current song if needed
    if (currentPlayingId && !song) {
      const savedSong = queue.find(item => item.queueId === currentPlayingId);
      if (savedSong) {
        setSong(savedSong);
      }
    }
  }, []);

  if (!song) {
    return <Typography variant="h5">Error: No song selected</Typography>;
  }

  const [instrumental, setInstrumental] = useState(false); // Track if instrumental is selected
  const [lastTimestamp, setLastTimestamp] = useState<number>(0); // Track last timestamp for KTV switch

  const currentQueueItem = useMemo(() => {
    return queue.find(item => item.queueId === currentPlayingId);
  }, [queue, currentPlayingId]);

  const COMBINED_DELIMITER = '<<<SEP>>>';

  const getCombinedLyrics = useMemo(() => {
    if (!song.lrcLyrics) {return 'Not supported';}

    const parseLyrics = (lyrics: string) => {
      const lines = lyrics.split('\n');
      const parsed: { time: string, text: string }[] = [];

      for (const line of lines) {
        const timeMatch = line.match(/^(\[[^\]]+\])/);
        if (timeMatch) {
          parsed.push({
            time: timeMatch[1],
            text: line.replace(timeMatch[1], '').trim()
          });
        }
      }
      return parsed;
    };

    const original = parseLyrics(song.lrcLyrics);
    const romanized = song.lrcRomanizedLyrics ? parseLyrics(song.lrcRomanizedLyrics) : [];
    const translated = song.lrcTranslatedLyrics ? parseLyrics(song.lrcTranslatedLyrics) : [];

    const combined: string[] = [];

    for (const origLine of original) {
      const romLine = romanized.find(l => l.time === origLine.time);
      const transLine = translated.find(l => l.time === origLine.time);

      const texts: string[] = [];

      // Always include the original
      if (origLine.text) {
        texts.push(origLine.text);
      }

      // Only include romanized if it's different from original (case insensitive)
      if (romLine && romLine.text && romLine.text.toLowerCase() !== origLine.text.toLowerCase()) {
        texts.push(romLine.text);
      }

      // Only include translation if it's different from original (case insensitive)
      if (transLine && transLine.text && transLine.text.toLowerCase() !== origLine.text.toLowerCase()) {
        texts.push(transLine.text);
      }

      const mergedText = texts.join(COMBINED_DELIMITER);
      combined.push(`${origLine.time}${mergedText}`);
    }

    return combined.join('\n');
  }, [song.lrcLyrics, song.lrcRomanizedLyrics, song.lrcTranslatedLyrics]);

  const handleKTVToggle = async () => {
    if (!song.id) { return; } // No song ID available

    // Store current timestamp before switching
    if (playerRef.current) {
      setLastTimestamp(playerRef.current.getCurrentTime());
    }

    try {
      if (!instrumental && !currentQueueItem?.ktvYouTubeId) {
        const response = await songApi.apiSongsSongIdVideoInstrumentalPut({
          songId: song.id
        });
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
      const errorMessage = err instanceof Error
        ? `Failed to get instrumental version. Error message from OpenAPI stub call: ${err.message}`
        : 'Failed to get instrumental version';
      setError(errorMessage);
      setInstrumental(false); // Revert toggle if error
    }

  };

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
  const currentLineRef = useRef(-1); // Track current line index\
  const lyricsOffsetRef = useRef<number>(lyricsOffset);
  useEffect(() => {
    lyricsOffsetRef.current = lyricsOffset;
  }, [lyricsOffset]);
  const checkIfTimeLineChanged = (currentTime: number, offset: number) => {
    currentTime += offset; // Apply offset to current time
    if (lrcTimestamps.length === 0 ||
      (currentTime >= prevTimeRange.current.start &&
        currentTime < prevTimeRange.current.end)) {
      return false;
    }
    for (let i = currentLineRef.current + 1; i < (lrcTimestamps.length + currentLineRef.current); i++) {
      i %= lrcTimestamps.length; // Wrap around if needed
      const currentTimestamp = lrcTimestamps[i];
      const nextTimestamp = ((i < lrcTimestamps.length - 1) ? lrcTimestamps[i + 1] : Infinity);
      if (currentTime >= currentTimestamp && currentTime < nextTimestamp) {
        currentLineRef.current = i;
        prevTimeRange.current = { start: currentTimestamp, end: nextTimestamp };
        return true;
      }
    }
    return false;
  };

  const prefetchNextSongs = useCallback(async () => {
    if (!queue.length) { return; }

    const currentIndex = queue.findIndex(item => item.queueId === currentPlayingId);
    if (currentIndex >= 0 && currentIndex < queue.length - 1) {
      // Get next 2 songs (or just 1 if we're at the end)
      const nextItems = queue.slice(currentIndex + 1, currentIndex + 3);

      nextItems.forEach((nextItem) => {
        // Only proceed if we haven't already requested the API for this song
        if (nextItem.status === 'pending') {
          // Mark as requested immediately to prevent duplicate calls
          nextItem.status = 'loading';

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

            setQueue(prevQueue => prevQueue.map(queueItem =>
              queueItem.queueId === nextItem.queueId
                ? {
                  ...queueItem,
                  title: response.title || nextItem.title,
                  artist: response.artist || nextItem.artist,
                  lyrics: nextItem.lyrics || '',
                  status: 'loaded' as const,
                  imageUrl: response.geniusMetaData?.songImageUrl ?? ''
                }
                : queueItem
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

  const animationFrameRef = useRef<number | null>(null);
  const prevOffset = useRef<number>(0);
  // Cleanup function for the animation frame
  const stopAnimationFrame = useCallback(() => {
    if (animationFrameRef.current) {
      cancelAnimationFrame(animationFrameRef.current);
    }
  }, []);

  const updatePlayerTime = useCallback((playerInstance: YouTubePlayer) => {
    playerRef.current = playerInstance;
    playerInstance.playVideo(); // Autoplay
    setShowQuizButton(isLanguageAllowedForQuiz ?? true);

    const updateLoop = () => {
      if (!playerRef.current) {
        return;
      }

      const current = playerRef.current.getCurrentTime();
      const duration = playerRef.current.getDuration();
      const offset = lyricsOffsetRef.current; //prev getting stale lyricsoffset cuz its nested updateloop (callback)
      if (checkIfTimeLineChanged(current, offset) || prevOffset.current !== offset) {
        setCurrentTime(current);
        prevOffset.current = offset;
      }

      // Check for quiz button show condition
      // if (current / duration >= 0.9 && isLanguageAllowedForQuiz) {
      // setShowQuizButton(true);
      // }

      // Check for prefetch condition
      if (current / duration >= 0.5) {
        prefetchNextSongs();
      }

      animationFrameRef.current = requestAnimationFrame(updateLoop);
    };

    updateLoop();
  }, [stopAnimationFrame, prefetchNextSongs, isLanguageAllowedForQuiz, lyricsOffset]);

  // Cleanup effect
  useEffect(() => {
    return () => {
      stopAnimationFrame();
    };
  }, [stopAnimationFrame]);

  // Reset function when song changes
  const resetLyricState = useCallback(() => {
    currentLineRef.current = -1;
    prevTimeRange.current = { start: Infinity, end: 0 };
    setCurrentTime(0);
    setShowQuizButton(false);
    setLyricsOffset(0);
    setMinLyricOffset(-1);
    setMaxLyricOffset(1);
  }, [song]);

  // Call resetLyricState when song changes
  useEffect(() => {
    resetLyricState();
  }, [song, resetLyricState]);

  const handleTabChange = (_event: React.SyntheticEvent, newValue: number) => {
    setSelectedTab(newValue);
  };

  const handleAddToNext = async () => {
    await handleQueueAddition(true);
    setInstrumental(false); // Set KTV to unselected if new song
  };

  const handleAddToEnd = async () => {
    await handleQueueAddition(false);
  };

  const handleQuizRedirect = () => {
    if (!song.id) {
      // console.warn('[SongPlayerPage] Cannot start quiz – song.id is empty');
      return;
    }

    // Clear any stale questions so the quiz page fetches fresh data
    setQuizQuestions([]);

    // Pass both songId and current lyricsOffset in the URL
    navigate(
      `/quiz?id=${encodeURIComponent(song.id)}&offset=${encodeURIComponent(
        lyricsOffset
      )}`
    );
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

      const newItem: QueueItem = {
        queueId: uuidv4(),
        status: 'loaded',
        title: response.title ?? '',
        artist: response.artist ?? '',
        youTubeId: response.youTubeId ?? '',
        lyrics: lyrics,
        imageUrl: response.geniusMetaData?.songImageUrl ?? ''
      };

      if (!response.lrcLyrics) {
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

      // Clear form
      setSongName('');
      setArtistName('');
      setLyrics('');
      setYoutubeUrl('');

    } catch (err) {
      const errorMessage = err instanceof Error
        ? `Failed to process song. Error message from OpenAPI stub call: ${err.message}`
        : 'Failed to process song';
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
        status: 'pending'
      }));

      // Add to end of queue
      setQueue(prev => [...prev, ...newItems]);
      setPlaylistUrl('');

    } catch (err) {
      const errorMessage = err instanceof Error
        ? `Failed to load playlist. Error message from OpenAPI stub call: ${err.message}`
        : 'Failed to load playlist';
      setError(errorMessage);
    } finally {
      setPlaylistLoading(false);
    }
  };

  const handlePlayFromQueue = useCallback(async (item: QueueItem) => {
    setError('');

    try {
      if (item.status === 'pending') {
        item.status = 'loading';
        setQueue(prevQueue => prevQueue.map(queueItem =>
          queueItem.queueId === item.queueId
            ? { ...queueItem, apiRequested: true, error: undefined }
            : queueItem
        ));

        const response = await songApi.apiSongsMatchPost({
          fullSongRequestDto: {
            title: item.title,
            artist: item.artist,
            youTubeId: item.youTubeId || '',
            lyrics: item.lyrics || ''
          }
        });
        if (item.youTubeId) {
          response.youTubeId = item.youTubeId;
        }

        setQueue(prevQueue => prevQueue.map(queueItem =>
          queueItem.queueId === item.queueId
            ? {
              ...queueItem,
              title: response.title || item.title,
              artist: response.artist || item.artist,
              lyrics: item.lyrics || '',
              status: 'loaded' as const,
              imageUrl: response.geniusMetaData?.songImageUrl ?? ''
            }
            : queueItem
        ));

      }
      else if (item.status === 'loaded') {
        const response = await songApi.apiSongsMatchPost({
          fullSongRequestDto: {
            title: item.title,
            artist: item.artist,
            youTubeId: item.youTubeId || '',
            lyrics: item.lyrics || ''
          }
        });
        if (item.youTubeId) {
          response.youTubeId = item.youTubeId;
        }
        setCurrentPlayingId(item.queueId);
        item.status = 'loaded';
        setSong(response);
        setInstrumental(false);
        stopAnimationFrame(); // Stop any current animation frame
        resetLyricState(); // Reset lyric state
      }
      else if (item.error) {
        setCurrentPlayingId(item.queueId);
        setSong({
          title: item.title,
          artist: item.artist,
          youTubeId: item.youTubeId || '',
          lrcLyrics: '',
          lrcRomanizedLyrics: '',
          lrcTranslatedLyrics: ''
        });
      }
    } catch (err) {
      const errorMessage = err instanceof Error
        ? `Failed to load song details. Error message from OpenAPI stub call: ${err.message}`
        : 'Failed to load song details';
      setQueue(prevQueue => prevQueue.map(queueItem =>
        queueItem.queueId === item.queueId
          ? { ...queueItem, error: errorMessage }
          : queueItem
      ));
    }
  }, [currentPlayingId, queue]);

  const [isFirst, setIsFirst] = useState(true);
  const [isLast, setIsLast] = useState(true);

  // Update first/last status when queue or currentPlayingId changes
  useEffect(() => {
    if (queue.length === 0) {
      setIsFirst(true);
      setIsLast(true);
      return;
    }

    const currentIndex = queue.findIndex(item => item.queueId === currentPlayingId);
    setIsFirst(currentIndex <= 0);
    setIsLast(currentIndex >= queue.length - 1);
  }, [currentPlayingId]);

  const handleTrackNavigation = (direction: 'prev' | 'next') => {
    if (queue.length === 0) { return; }

    const currentIndex = queue.findIndex(item => item.queueId === currentPlayingId);
    if (currentIndex === -1) { return; }

    let newIndex;
    if (direction === 'prev') {
      newIndex = Math.max(0, currentIndex - 1);
    } else {
      newIndex = Math.min(queue.length - 1, currentIndex + 1);
    }

    const newItem = queue[newIndex];

    handlePlayFromQueue(newItem);

    // Reset KTV mode when changing tracks
    setInstrumental(false);
  };

  const handlePrevTrack = () => handleTrackNavigation('prev');
  const handleNextTrack = () => handleTrackNavigation('next');

  const CompactInput = styled(MuiInput)`
  width: 43px;
  margin: 0 8px;

  & input {
    padding: 2px 4px;
    font-size: 0.875rem;
  }
`;

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
          <Grid size={6} alignContent={'center'} className='grid-parent'>
            <YouTubePlayer
              videoId={instrumental && currentQueueItem?.ktvYouTubeId ? currentQueueItem.ktvYouTubeId : song.youTubeId ?? ''}
              onReady={(playerInstance) => {
                updatePlayerTime(playerInstance);
                if (lastTimestamp > 0) {
                  playerInstance.seekTo(lastTimestamp, true);
                }
              }}
              autoStart={true}
              onEnd={() => {
                if (autoPlayEnabled) {
                  handleNextTrack();
                }
              }}
              key={instrumental ? 'ktv' : 'regular'} // Force re-render when switching
            />
            <Grid container spacing={2} className="controls-grid">
              <Grid size={1} alignContent={'center'}>
                <ToggleButton value="check" selected={instrumental} onChange={handleKTVToggle} className="ktv-toggle">
                  KTV
                </ToggleButton>
              </Grid>
              <Grid size={1} alignContent={'center'}>
                <IconButton
                  className="queue-nav-button"
                  onClick={handlePrevTrack}
                  disabled={isFirst}
                  size="large"
                >
                  <SkipPreviousIcon />
                </IconButton>
              </Grid>
              <Grid size={1} alignContent={'center'}>
                <IconButton
                  className="queue-nav-button"
                  onClick={handleNextTrack}
                  disabled={isLast}
                  size="large"
                >
                  <SkipNextIcon />
                </IconButton>
              </Grid>
              <Grid size={8} className='slider-grid-parent'>
                <Grid container spacing={0} alignItems="center" justifyContent="center" className='slider-grid'>
                  <Grid size={2}>
                    <CompactInput
                      value={minLyricOffset}
                      size="small"
                      onChange={(e: React.ChangeEvent<HTMLInputElement>) => {
                        const newMin = Number(e.target.value);
                        if (newMin < maxLyricOffset) {
                          setMinLyricOffset(newMin);
                          if (lyricsOffset < newMin) { setLyricsOffset(newMin); }
                        }
                      }}
                      onBlur={() => {
                        if (minLyricOffset >= maxLyricOffset) { setMinLyricOffset(maxLyricOffset - 1); }
                      }}
                      inputProps={{
                        step: 1,
                        min: -99,
                        max: 98,
                        type: 'number',
                      }}
                    />
                  </Grid>
                  <Grid size={6} className='slider'>
                    <Slider
                      value={lyricsOffset}
                      onChange={(_e, value) => {
                        setLyricsOffset(Number(value));
                      }}
                      min={minLyricOffset}
                      max={maxLyricOffset}
                      step={0.01}
                      color='primary'
                      valueLabelDisplay="auto"
                      valueLabelFormat={(value) => `${value > 0 ? '+' : ''}${value}s`}
                    />
                  </Grid>
                  <Grid size={1} className="slider-label-box">
                    <CompactInput
                      value={maxLyricOffset}
                      size="small"
                      onChange={(e) => {
                        const newMax = Number(e.target.value);
                        if (newMax > minLyricOffset) {
                          setMaxLyricOffset(newMax);
                          if (lyricsOffset > newMax) { setLyricsOffset(newMax); }
                        }
                      }}
                      onBlur={() => {
                        if (maxLyricOffset <= minLyricOffset) { setMaxLyricOffset(minLyricOffset + 1); }
                      }}
                      inputProps={{
                        step: 1,
                        min: -98,
                        max: 99,
                        type: 'number',
                      }}
                    />
                  </Grid>
                  <Grid size={3} className="lyric-set-offset">
                    <TextField
                      label="Offset"
                      type="number"
                      size="small"
                      value={lyricsOffset}
                      onChange={(e) => {
                        const newValue = Number(e.target.value);
                        if (newValue >= minLyricOffset && newValue <= maxLyricOffset) {
                          setLyricsOffset(newValue);
                        }
                      }}
                      className='slider-boxes'
                      inputProps={{
                        step: 0.02,
                        style: { textAlign: 'center' },
                      }}
                    />
                  </Grid>
                </Grid>
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
                <Tab label="Combined Lyrics" />
              </Tabs>
            </Box>
            <Box className='lrc-grid-parent'>
              <LyricDisplay
                rawLrcLyrics={
                  selectedTab === 0
                    ? song.lrcLyrics ?? 'Not supported'
                    : selectedTab === 1
                      ? song.lrcRomanizedLyrics ?? 'Not supported'
                      : selectedTab === 2
                        ? song.lrcTranslatedLyrics ?? 'Not supported'
                        : getCombinedLyrics
                }
                currentTime={currentTime + lyricsOffsetRef.current}
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
                  <Tooltip title="Add to next in queue">
                    <IconButton
                      color="primary"
                      onClick={handleAddToNext}
                      disabled={isLoading}
                      className="queue-button"
                      size="large"
                    >
                      <AddIcon fontSize="inherit" />
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
          <Grid size={4} className="queue-grid">
            <QueueComponent
              queue={queue}
              currentPlayingId={currentPlayingId}
              setQueue={setQueue}
              setCurrentPlayingId={setCurrentPlayingId}
              autoPlayEnabled={autoPlayEnabled}
              handlePlayFromQueue={handlePlayFromQueue}
              setAutoPlayEnabled={setAutoPlayEnabled}
            />
          </Grid>
        </Grid>
      </Container>
    </div >
  );
};

export default SongPlayerPage;
