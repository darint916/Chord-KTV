import React, { useState, useRef, useEffect } from 'react';
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

  if (!song) {
    return <Typography variant="h5">Error: No song selected</Typography>;
  }
  
  const allowedQuizLanguages = new Set(['AR', 'BG', 'BN', 'EL', 'FA', 'GU', 'HE', 'HI', 'JA', 'KO', 'RU', 'SR', 'TA', 'TE', 'TH', 'UK', 'ZH']);
  const isLanguageAllowedForQuiz = song.geniusMetaData?.language && allowedQuizLanguages.has(song.geniusMetaData.language);

  const handlePlayerReady = (playerInstance: YouTubePlayer) => {
    playerRef.current = playerInstance;

    setInterval(() => {
      if (playerRef.current) { 
        const current = playerRef.current.getCurrentTime();
        const duration = playerRef.current.getDuration();
        setCurrentTime(current);

        // Check if the song is 90% complete for quiz button
        if (current / duration >= 0.9 && isLanguageAllowedForQuiz) {
          setShowQuizButton(true);
        }

        // Check if we're at the halfway point and there's a next song in queue
        if (current / duration >= 0.5 && queue.length > 1 && currentPlayingId) {
          const currentIndex = queue.findIndex(item => item.queueId === currentPlayingId);
          if (currentIndex >= 0 && currentIndex < queue.length - 1) {
            const nextItem = queue[currentIndex + 1];
            console.log(nextItem);
            
            // Only proceed if we haven't already requested the API for this song
            if (!nextItem.apiRequested) {
              // Mark as requested immediately to prevent duplicate calls
              nextItem.apiRequested = true;

              // Call the API for the next song
              songApi.apiSongsSearchPost({
                fullSongRequestDto: {
                  title: nextItem.title,
                  artist: nextItem.artist,
                  youTubeId: extractYouTubeVideoId(nextItem.youtubeUrl) || '',
                  lyrics: nextItem.lyrics || ''
                }
              }).then(response => {
                // Update the queue with the processed data
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
                          geniusMetaData: response.geniusMetaData
                        }
                      } 
                    : item
                ));
              }).catch(err => {
                console.error('Failed to pre-process next song:', err);
                // Even if it fails, we keep apiRequested as true to prevent retries
              });
            }
          }
        }
      }
    }, 1000);
  };

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
        youtubeUrl: youtubeUrl,
        apiRequested: false
      };

      setQueue(prev => [...prev, newItem]);
      
      // If nothing is currently playing, play this new item
      if (!currentPlayingId) {
        setCurrentPlayingId(newItem.queueId);
        if (youTubeId) {
          // Minimal song data for immediate playback
          setSong({
            title: songName,
            artist: artistName,
            youTubeId: youTubeId,
            lrcLyrics: '',
            lrcRomanizedLyrics: '',
            lrcTranslatedLyrics: ''
          });
        }
      }

      // Clear form
      setSongName('');
      setArtistName('');
      setLyrics('');
      setYoutubeUrl('');

    } catch (err) {
      setError('Search failed. Please try again.');
    } finally {
      setIsLoading(false);
    }
  };

  const moveQueueItem: (dragIndex: number, hoverIndex: number) => void = 
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
    setIsLoading(true);
    setError('');

    try {
      const vidId = extractYouTubeVideoId(item.youtubeUrl);
      
      // Only proceed with API call if not already requested
      if (!item.apiRequested) {
        item.apiRequested = true;

        const response = await songApi.apiSongsSearchPost({
          fullSongRequestDto: {
            title: item.title,
            artist: item.artist,
            youTubeId: vidId || '',
            lyrics: item.lyrics || ''
          }
        });

        // Update the current playing song with the API response
        const processedData = {
          title: response.title,
          artist: response.artist,
          youTubeId: response.youTubeId,
          lrcLyrics: response.lrcLyrics,
          lrcRomanizedLyrics: response.lrcRomanizedLyrics,
          lrcTranslatedLyrics: response.lrcTranslatedLyrics,
          geniusMetaData: response.geniusMetaData
        };

        // Update the queue item with processed data for future reference
        setQueue(prevQueue => prevQueue.map(queueItem => 
          queueItem.queueId === item.queueId 
            ? { ...queueItem, processedData, apiRequested: true } 
            : queueItem
        ));

        // Set as current song
        setCurrentPlayingId(item.queueId);
        setSong(processedData);
      } else {
        // If already requested, just use the existing processed data or basic info
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
      setError('Failed to load song details. Playing with basic info.');
      
      // Fallback to basic playback if API fails
      const vidId = extractYouTubeVideoId(item.youtubeUrl);
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
        <Grid container className="song-player-content">
          
          {/* Video Player Column */}
          <Grid className='youtube-grid-parent'>
            <YouTubePlayer videoId={song.youTubeId ?? ''} onReady={handlePlayerReady} />
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
