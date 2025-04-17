import React, { useState, useRef, useEffect } from 'react';
import { Container, Typography, Box, Button, Paper, TextField, IconButton, CircularProgress, Alert, List, ListItem, ListItemText, Divider } from '@mui/material';
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
import YouTubePlaylistViewer from '../../components/YouTubePlaylistViewer/YouTubePlaylistViewer';
import { useLocation } from 'react-router-dom';
import { songApi } from '../../api/apiClient';
import ListItemButton from '@mui/material/ListItemButton';
import { FullSongResponseDto } from '../../api';
import { v4 as uuidv4 } from 'uuid';
import DeleteIcon from '@mui/icons-material/Delete';

// Define the YouTubePlayer interface
interface YouTubePlayer {
  seekTo: (_seconds: number, _allowSeekAhead: boolean) => void;
  playVideo: () => void;
  pauseVideo: () => void;
  getCurrentTime: () => number;
  getDuration: () => number;
  setVolume: (_volume: number) => void;
}

interface QueueItem extends FullSongResponseDto {
  queueId: string;
}

const SongPlayerPage: React.FC = () => {
  const [currentTime, setCurrentTime] = useState<number>(0);
  const [isPlaying] = useState<boolean>(false);
  const playerRef = useRef<YouTubePlayer | null>(null);
  const { song, setQuizQuestions, setSong } = useSong();
  const [selectedTab, setSelectedTab] = useState(0);
  const [showQuizButton, setShowQuizButton] = useState(false);
  const navigate = useNavigate();
  const location = useLocation();
  const { playlistUrl } = location.state || {};
  const [songName, setSongName] = useState('');
  const [isLoading, setIsLoading] = useState(false);
  const [artistName, setArtistName] = useState('');
  const [lyrics, setLyrics] = useState('');
  const [error, setError] = useState('');
  const [youtubeUrl, setYoutubeUrl] = useState('');

  const [queue, setQueue] = useState<QueueItem[]>(() => {
    const savedQueue = typeof window !== 'undefined' ? localStorage.getItem('songQueue') : null;
    return savedQueue ? JSON.parse(savedQueue) : [];
  });

  const [currentPlayingId, setCurrentPlayingId] = useState<string | null>(() => {
    const savedCurrentId = typeof window !== 'undefined' ? localStorage.getItem('currentPlayingId') : null;
    return savedCurrentId ? JSON.parse(savedCurrentId) : null;
  });

  useEffect(() => {
    // Restore current song if needed
    if (currentPlayingId && !song) {
      const savedSong = queue.find(item => item.queueId === currentPlayingId);
      if (savedSong) {
        setSong(savedSong);
      }
    }
  }, []);

  const saveQueueState = (queue: FullSongResponseDto[], currentId: string | null) => {
    if (typeof window !== 'undefined') {
      localStorage.setItem('songQueue', JSON.stringify(queue));
      localStorage.setItem('currentPlayingId', JSON.stringify(currentId));
    }
  };

  if (!song) {
    return <Typography variant="h5">Error: No song selected</Typography>;
  }
  
  if (!song.lrcLyrics || !song.lrcLyrics.trim()) {
    return <Typography variant="h5">Error: No time-synced lyrics found for song</Typography>;
  }

  if (!song.youTubeId || !song.youTubeId.trim()) {
    return <Typography variant="h5">Error: No YouTube video found for song</Typography>;
  }

  const allowedQuizLanguages = new Set(['AR', 'BG', 'BN', 'EL', 'FA', 'GU', 'HE', 'HI', 'JA', 'KO', 'RU', 'SR', 'TA', 'TE', 'TH', 'UK', 'ZH']);
  const isLanguageAllowedForQuiz = song.geniusMetaData?.language && allowedQuizLanguages.has(song.geniusMetaData.language);

  const handlePlayerReady = (playerInstance: YouTubePlayer) => {
    playerRef.current = playerInstance;

    setInterval(() => {
      if (playerRef.current) { 
        const current = playerRef.current.getCurrentTime();
        setCurrentTime(current);

        // Check if the song is 90% complete
        if (current / playerRef.current.getDuration() >= 0.9 && isLanguageAllowedForQuiz) {
          setShowQuizButton(true); // Show the quiz button when 90% complete
        }
      }
    }, 1); // Noticed player was falling behind, 1ms for absolute accuracy
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

    const youTubeId = extractYouTubeVideoId(youtubeUrl);

    try {
      const response = await songApi.apiSongsSearchPost({
        fullSongRequestDto: {
          title: songName,
          artist: artistName,
          lyrics: lyrics,
          youTubeId: youTubeId || ''
        }
      });

      // Add to queue
      if (response) {
        const newQueue = [...queue, { ...response, queueId: uuidv4() }];
        setQueue(newQueue);
        saveQueueState(newQueue, currentPlayingId); 
      };
    } catch {
      setError('Search failed. Please try again.');
    } finally {
      setIsLoading(false); // Set loading state to false when the search finishes
    }
  };

  const handleKeyDown = (event: React.KeyboardEvent<HTMLDivElement>) => {
    if (event.key === 'Enter') {
      handleQueueAddition();
    }
  };

  const handlePlayFromQueue = (item: QueueItem) => {
    setSong(item); 
    setCurrentPlayingId(item.queueId);
    saveQueueState(queue, item.queueId);
  };

  const clearQueue = () => {
    if (!currentPlayingId) {
      setQueue([]);
      setCurrentPlayingId(null);
      saveQueueState([], null);
      return;
    }

    const currentSong = queue.find(item => item.queueId === currentPlayingId);
    if (currentSong) {
      setQueue([currentSong]);
      saveQueueState([currentSong], currentPlayingId);
    } else {
      setQueue([]);
      setCurrentPlayingId(null);
      saveQueueState([], null);
    }
  };

  const removeFromQueue = (queueId: string) => {
    const newQueue = queue.filter(item => item.queueId !== queueId);
    setQueue(newQueue);
    
    saveQueueState(newQueue, currentPlayingId === queueId ? null : currentPlayingId);
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
          <Grid className="queue-column" component={Paper}>
              <Typography variant="h6" className="queue-title" align="center">
                Queue
              </Typography>
              <Divider />
              <List className="queue-list">
              {queue.map((item, index) => (
                <React.Fragment key={item.queueId}>
                  <ListItemButton 
                    onClick={() => handlePlayFromQueue(item)}
                    className={`queue-item ${currentPlayingId === item.queueId ? 'active-song' : ''}`}
                    sx={{
                      backgroundColor: currentPlayingId === item.queueId ? 'rgba(25, 118, 210, 0.08)' : 'transparent',
                      '&:hover': {
                        backgroundColor: currentPlayingId === item.queueId 
                          ? 'rgba(25, 118, 210, 0.12)' 
                          : 'rgba(255, 255, 255, 0.1)'
                      }
                    }}
                  >
                    <ListItemText 
                      primary={`${index + 1}. ${item.title}`} 
                      secondary={item.artist}
                      primaryTypographyProps={{ 
                        noWrap: true,
                        fontWeight: currentPlayingId === item.queueId ? 'bold' : 'normal'
                      }}
                      secondaryTypographyProps={{ noWrap: true }}
                    />
                    {currentPlayingId === item.queueId && (
                      <Box sx={{ 
                        width: 8, 
                        height: 8, 
                        borderRadius: '50%', 
                        bgcolor: 'primary.main',
                        ml: 1
                      }} />
                    )}
                    {/* Only show delete button if NOT the current playing song */}
                    {currentPlayingId !== item.queueId && (
                      <IconButton
                        edge="end"
                        aria-label="remove"
                        onClick={(e) => {
                          e.stopPropagation();
                          removeFromQueue(item.queueId);
                        }}
                        sx={{
                          color: 'error.main',
                          '&:hover': {
                            backgroundColor: 'rgba(255, 0, 0, 0.1)'
                          }
                        }}
                      >
                        <DeleteIcon />
                      </IconButton>
                    )}
                  </ListItemButton>
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
        </Grid>
        {playlistUrl && <YouTubePlaylistViewer playlistUrl={playlistUrl} />}
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
