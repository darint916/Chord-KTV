import React, { useState } from 'react';
import {
  Box,
  Typography,
  TextField,
  IconButton,
  Alert,
  Paper,
  Container,
  CircularProgress,
  Button
} from '@mui/material';
import { useNavigate } from 'react-router-dom';
import { useSong } from '../../contexts/SongContext';
import SearchIcon from '@mui/icons-material/Search';
import { useAuth } from '../../contexts/authTypes';
import axios from 'axios';
import YouTubePlaylistViewer from '../../components/YouTubePlaylistViewer/YouTubePlaylistViewer';
import './HomePage.scss';
import { songApi } from '../../api/apiClient';
import logo from '../../assets/chordktv.png';

const HomePage: React.FC = () => {
  const { user } = useAuth();
  const [songName, setSongName] = useState('');
  const [artistName, setArtistName] = useState('');
  const [error, setError] = useState('');
  const [isLoading, setIsLoading] = useState(false);
  const navigate = useNavigate();
  const { setSong } = useSong();
  const [playlistUrl, setPlaylistUrl] = useState('');
  const [showPlaylist, setShowPlaylist] = useState(false); 
  const [lyrics, setLyrics] = useState('');
  const [youtubeUrl, setYoutubeUrl] = useState('');

  const handleKeyDown = (event: React.KeyboardEvent<HTMLDivElement>) => {
    if (event.key === 'Enter') {
      handleSearch();
    }
  };

  const extractYouTubeVideoId = (url: string | null | undefined): string | null => {
    if (!url) {return null;}
    const match = url.match(/(?:\?v=|\/embed\/|\.be\/|\/watch\?v=|\/watch\?.+&v=)([a-zA-Z0-9_-]{11})/);
    return match ? match[1] : null;
  };

  const handleSearch = async () => {
    if (!songName.trim() && !artistName.trim() && !lyrics.trim() && !youtubeUrl.trim()) {
      setError('Please enter at least one field to search.');
      return;
    }

    setIsLoading(true);

    const youTubeId = extractYouTubeVideoId(youtubeUrl);

    if (user) {
      try {
        await axios.post('http://localhost:5259/api/random', 
          {
            songName,
            artistName,
            timestamp: new Date()
          },
          {
            headers: {
              'Authorization': `Bearer ${user.idToken}`
            }
          }
        );
        setError('');
      } catch {
        setError('Failed to save search history. Please try again.');
      }
    }

    try {
      const response = await songApi.apiSongsSearchPost({
        fullSongRequestDto: {
          title: songName,
          artist: artistName,
          lyrics: lyrics,
          youTubeId: youTubeId || ''
        }
      });
      setSong(response);
      navigate('/play-song');
    } catch {
      setError('Search failed. Please try again.');
    } finally {
      setIsLoading(false); // Set loading state to false when the search finishes
    }
  };

  const handleLoadPlaylist = () => {
    if (!playlistUrl.trim()) {
      setError('Please enter a valid YouTube playlist URL.');
      return;
    }
    setShowPlaylist(true);
  };

  return (
    <Box className="home-page">
      <Container maxWidth="md">
        <img src={logo} alt="Logo" className="resized-image" />
        {error && (
          <Alert severity="error" className="error-alert">
            {error}
          </Alert>
        )}

        {/* Search Section */}
        <Paper elevation={3} className="search-section">
          <Typography variant="h5" className="section-title">
            Search for a Song
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
              onClick={handleSearch}
              disabled={isLoading}
              className={`search-button ${isLoading ? 'loading' : ''}`}
              size="large"
            >
              {isLoading ? <CircularProgress size={24} /> : <SearchIcon />}
            </IconButton>
          </Box>
        </Paper>

        {/* OR Divider */}
        <Box className="or-divider">
          <Paper elevation={3} className="or-paper">
            <Typography variant="body1" className="or-text">
              OR
            </Typography>
          </Paper>
        </Box>

        {/* YouTube Playlist Section */}
        <Paper elevation={3} className="playlist-section">
          <Typography variant="h5" className="section-title">
            Load a YouTube Playlist
          </Typography>
          <div className='playlist-url-input'>
            <TextField
              fullWidth
              label="Enter YouTube Playlist URL"
              variant="filled"
              value={playlistUrl}
              onKeyDown={(e) => {
                if (e.key === 'Enter') {
                  handleLoadPlaylist();
                }
              }}
              onChange={(e) => setPlaylistUrl(e.target.value)}
            />
            <Button
              variant="contained"
              onClick={handleLoadPlaylist}
              disabled={isLoading}
            >
              Load Playlist
            </Button>
          </div>
          {showPlaylist && <YouTubePlaylistViewer playlistUrl={playlistUrl} />}
        </Paper>
      </Container>
    </Box>
  );
};

export default HomePage;