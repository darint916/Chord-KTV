import React, { useState, useRef, useEffect, useMemo } from 'react';
import { Container, Typography, Box, Button, Paper, TextField, IconButton, CircularProgress, Alert, List, Divider } from '@mui/material';
import SearchIcon from '@mui/icons-material/Search';
import ClearAll from '@mui/icons-material/Search';
import YouTubePlayer from '../../components/YouTubePlayer/YouTubePlayer';
import LyricDisplay from '../../components/LyricDisplay/LyricDisplay';
import './SongPlayerPage.scss';
import Grid from '@mui/material/Grid2';
import Tabs from '@mui/material/Tabs';
import Tab from '@mui/material/Tab';
import { useSong } from '../../contexts/SongContext';
import { useNavigate } from 'react-router-dom';
import { songApi } from '../../api/apiClient';
import { v4 as uuidv4 } from 'uuid';
import { DndProvider } from 'react-dnd';
import { HTML5Backend } from 'react-dnd-html5-backend';
import DraggableQueueItem from '../../components/DraggableQueueItem/DraggableQueueItem';
import { QueueItem } from '../../contexts/QueueTypes';

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

  if (!song.lrcLyrics || !song.lrcLyrics.trim()) {
    return <Typography variant="h5">Error: No time-synced lyrics found for song</Typography>;
  }

  if (!song.youTubeId || !song.youTubeId.trim()) {
    return <Typography variant="h5">Error: No YouTube video found for song</Typography>;
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
    return timestamps.sort((a, b) => a - b);
  }, [song.lrcLyrics]);


  const prevTimeRange = useRef({ start: Infinity, end: 0 });
  const checkIfTimeLineChanged = (currentTime: number, timestamps: number[]) => {
    if (timestamps.length === 0 || (currentTime >= prevTimeRange.current.start && currentTime < prevTimeRange.current.end)) {
      return false;
    }
    for (let i = 0; i < timestamps.length; i++) {
      const currentTimestamp = timestamps[i];
      const nextTimestamp = (i < timestamps.length - 1) ? timestamps[i + 1] : Infinity;
      if (currentTime >= currentTimestamp && currentTime < nextTimestamp) {
        prevTimeRange.current = { start: currentTimestamp, end: nextTimestamp };
        break;
      }
    }
    return true;
  };

  const allowedQuizLanguages = new Set(['AR', 'BG', 'BN', 'EL', 'FA', 'GU', 'HE', 'HI', 'JA', 'KO', 'RU', 'SR', 'TA', 'TE', 'TH', 'UK', 'ZH']);
  const isLanguageAllowedForQuiz = song.geniusMetaData?.language && allowedQuizLanguages.has(song.geniusMetaData.language);

  const updatePlayerTime = (playerInstance: YouTubePlayer) => {
    playerRef.current = playerInstance;
    playerInstance.playVideo(); // Autoplay

    const updatePlayerTime = () => {
      if (playerRef.current) {
        const current = playerRef.current.getCurrentTime();
        if (checkIfTimeLineChanged(current, lrcTimestamps)) {
          setCurrentTime(current);
        }
        // Check if the song is 90% complete
        if (current / playerRef.current.getDuration() >= 0.9 && isLanguageAllowedForQuiz) {
          setShowQuizButton(true); // Show the quiz button when 90% complete
        }
      }
      animationFrameId = requestAnimationFrame(updatePlayerTime); //req next frame
    };

    updatePlayerTime();
  };

  useEffect(() => { //cleanup on rerender
    return () => {
      if (animationFrameId) {
        cancelAnimationFrame(animationFrameId); // Cancel the animation frame when the component unmounts  (cleanup function)
      };
    };
  }, []);

  const handleTabChange = (_event: React.SyntheticEvent, newValue: number) => {
    setSelectedTab(newValue);
  };

  const handleQuizRedirect = () => {
    setQuizQuestions([]);   // Clear old song quiz questions
    navigate('/quiz');
  };
  
  const extractYouTubeVideoId = (url: string | null | undefined): string | null => {
    if (!url) {return null;}
    const match = url.match(/(?:\?v=|\/embed\/|\.be\/|\/watch\?v=|\/watch\?.+&v=)([a-zA-Z0-9_-]{11})/);
    return match ? match[1] : null;
  };

  const handleQueueAddition = async () => {
    if (!songName.trim() && !artistName.trim() && !lyrics.trim() && !youtubeUrl.trim()) {
      setError('Please enter at least one field to search.');
      return;
    }

    setIsLoading(true);
    setError('');

    try {
      const youTubeId = extractYouTubeVideoId(youtubeUrl);
      const newItem: QueueItem = {
        queueId: uuidv4(),
        title: songName,
        artist: artistName,
        lyrics: lyrics,
        youTubeId: youTubeId ?? '',
        apiRequested: false
      };

      setQueue(prev => [...prev, newItem]);

      // Clear form
      setSongName('');
      setArtistName('');
      setLyrics('');
      setYoutubeUrl('');

    } catch {
      setError('Search failed. Please try again.');
    } finally {
      setIsLoading(false);
    }
  };

  const moveQueueItem: (_dragIndex: number, _hoverIndex: number) => void = 
  (dragIndex, hoverIndex) => {
    setQueue((prevQueue: QueueItem[]) => {
      const newQueue = [...prevQueue];
      const [removed] = newQueue.splice(dragIndex, 1);
      newQueue.splice(hoverIndex, 0, removed);
      return newQueue;
    });
  };

  const handleKeyDown = (event: React.KeyboardEvent<HTMLDivElement>) => {
    if (event.key === 'Enter') {
      handleQueueAddition();
    }
  };

  const handlePlayFromQueue = async (item: QueueItem) => {
    if (item.error) {return;} // Don't try to play errored items

    setIsLoading(true);
    setError('');

    try {
      const vidId = extractYouTubeVideoId(item.youTubeId);
      
      if (!item.apiRequested) {
        // Mark as requested immediately
        setQueue(prevQueue => prevQueue.map(queueItem => 
          queueItem.queueId === item.queueId 
            ? { ...queueItem, apiRequested: true, error: undefined } 
            : queueItem
        ));

        const response = await songApi.apiSongsSearchPost({
          fullSongRequestDto: {
            title: item.title,
            artist: item.artist,
            youTubeId: vidId || '',
            lyrics: item.lyrics || ''
          }
        });

        const processedData = {
          title: response.title,
          artist: response.artist,
          youTubeId: response.youTubeId,
          lrcLyrics: response.lrcLyrics,
          lrcRomanizedLyrics: response.lrcRomanizedLyrics,
          lrcTranslatedLyrics: response.lrcTranslatedLyrics,
          geniusMetaData: response.geniusMetaData
        };

        setQueue(prevQueue => prevQueue.map(queueItem => 
          queueItem.queueId === item.queueId 
            ? { ...queueItem, processedData, apiRequested: true } 
            : queueItem
        ));

        setCurrentPlayingId(item.queueId);
        setSong(processedData);
      } else {
        const playbackData = item.processedData || {
          title: item.title,
          artist: item.artist,
          youTubeId: vidId || '',
          lrcLyrics: '',
          lrcRomanizedLyrics: '',
          lrcTranslatedLyrics: ''
        };
        
        setCurrentPlayingId(item.queueId);
        setSong(playbackData);
      }
    } catch (err) {
      const errorMessage = err instanceof Error ? err.message : 'Failed to load song details';
      
      // Update the queue item with error state
      setQueue(prevQueue => prevQueue.map(queueItem => 
        queueItem.queueId === item.queueId 
          ? { ...queueItem, error: errorMessage } 
          : queueItem
      ));
      
      // Fallback to basic playback
      const vidId = extractYouTubeVideoId(item.youTubeId);
      setCurrentPlayingId(item.queueId);
      setSong({
        title: item.title,
        artist: item.artist,
        youTubeId: vidId || '',
        lrcLyrics: '',
        lrcRomanizedLyrics: '',
        lrcTranslatedLyrics: ''
      });
    } finally {
      setIsLoading(false);
    }
  };

  const clearQueue = () => {
    if (!currentPlayingId) {
      setQueue([]);
      setCurrentPlayingId(null);
      return;
    }

    const currentSong = queue.find(item => item.queueId === currentPlayingId);
    if (currentSong) {
      setQueue([currentSong]);
    } else {
      setQueue([]);
      setCurrentPlayingId(null);
    }
  };

  const removeFromQueue = (queueId: string) => {
    const newQueue = queue.filter(item => item.queueId !== queueId);
    setQueue(newQueue);
  };

  return (
    <div className="song-player-page">
      <Container maxWidth="lg" className="song-player-container">
        <Typography variant="h3" className="song-title" align="center">
          {song.title}
        </Typography>
        <Typography variant="h5" className="song-title" align="center">
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
        <Grid container className="song-player-content" spacing={10} height={'480px'} display={'flex'}>
          {/* we use grid now as later plan to add additional column additions, change spacing if needed*/}
          <Grid flex={'1'} alignContent={'center'} className='grid-parent'>
            <YouTubePlayer videoId={song.youTubeId ?? ''} onReady={updatePlayerTime} />
          </Grid>
          {/* Lyrics Column */}
          <Grid className='grid-parent'>
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
                currentTime={currentTime}
                isPlaying={isPlaying}
              />
            </Box>
          </Grid>
          {/* Queue Column */}
          <DndProvider backend={HTML5Backend}>
            <Grid className="queue-column" component={Paper}>
              <Typography variant="h6" className="queue-title" align="center">
                Queue
              </Typography>
              <Divider />
              <List className="queue-list">
                {queue.map((item, index) => (
                  <React.Fragment key={item.queueId}>
                    <DraggableQueueItem 
                      item={item}
                      index={index}
                      moveItem={moveQueueItem}
                      onRemove={removeFromQueue}
                      onPlay={handlePlayFromQueue}
                      currentPlayingId={currentPlayingId}
                    />
                    {index < queue.length - 1 && <Divider />}
                  </React.Fragment>
                ))}
              </List>
              <Box mt={2} display="flex" gap={1}>
                <Button 
                  variant="outlined" 
                  onClick={clearQueue}
                  startIcon={<ClearAll />}
                >
                  Clear Queue
                </Button>
              </Box>
            </Grid>
          </DndProvider>
        </Grid>
        {error && (
          <Alert severity="error" className="error-alert">
            {error}
          </Alert>
        )}
          
        {/* Search Section */}
        <Paper elevation={3} className="search-section">
          <Typography variant="h5" className="section-title">
            Add a Song to the Queue
          </Typography>
          <Box display="flex" alignItems="center" gap={2}>
            <Box
              display="grid"
              gridTemplateColumns="1fr 1fr"
              gap={2}
              flexGrow={1}
            >
              <TextField
                label="Song Name"
                variant="filled"
                value={songName}
                disabled={isLoading}
                onKeyDown={handleKeyDown}
                onChange={(e) => setSongName(e.target.value)}
                className="search-input"
                fullWidth
              />
              <TextField
                label="Artist Name"
                variant="filled"
                value={artistName}
                disabled={isLoading}
                onKeyDown={handleKeyDown}
                onChange={(e) => setArtistName(e.target.value)}
                className="search-input"
                fullWidth
              />
              <TextField
                label="Lyrics"
                variant="filled"
                value={lyrics}
                disabled={isLoading}
                onKeyDown={handleKeyDown}
                onChange={(e) => setLyrics(e.target.value)}
                className="search-input"
                fullWidth
              />
              <TextField
                label="YouTube URL"
                variant="filled"
                value={youtubeUrl}
                disabled={isLoading}
                onKeyDown={handleKeyDown}
                onChange={(e) => setYoutubeUrl(e.target.value)}
                className="search-input"
                fullWidth
              />
            </Box>
            <IconButton
              aria-label="search"
              onClick={handleQueueAddition}
              disabled={isLoading}
              className={`search-button ${isLoading ? 'loading' : ''}`}
              size="large"
            >
              {isLoading ? <CircularProgress size={24} /> : <SearchIcon />}
            </IconButton>
          </Box>
        </Paper>
      </Container>
    </div>
  );
};

export default SongPlayerPage;
