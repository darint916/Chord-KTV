import React, { useState } from 'react';
import {
  Box,
  Typography,
  TextField,
  IconButton,
  Alert,
  Paper,
  Container,
  Stack,
  CircularProgress
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

  const handleKeyDown = (event: React.KeyboardEvent<HTMLDivElement>) => {
    if (event.key === 'Enter') {
      handleSearch();
    }
  };

  const handleSearch = async () => {
    if (!songName.trim() || !artistName.trim()) {
      setError('Please enter both a song name and an artist name.');
      return;
    }

    setIsLoading(true);

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
          artist: artistName
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
          <Stack direction="row" spacing={2} alignItems="center">
            <TextField
              label="Song Name"
              variant="filled"
              value={songName}
              disabled={isLoading}
              onKeyDown={handleKeyDown}
              onChange={(e) => setSongName(e.target.value)}
              fullWidth
              className="search-input"
            />
            <TextField
              label="Artist Name"
              variant="filled"
              disabled={isLoading} 
              value={artistName}
              onKeyDown={handleKeyDown}
              onChange={(e) => setArtistName(e.target.value)}
              fullWidth
              className="search-input"
            />
            <IconButton
              aria-label="search"
              onClick={handleSearch}
              disabled={isLoading}
              className="search-button"
            >
              {isLoading ? <CircularProgress size={24} /> : <SearchIcon />}
            </IconButton>
          </Stack>
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
          <YouTubePlaylistViewer />
        </Paper>

        {user && (
          <Typography variant="body1" className="logged-in-message">
            Logged in as: {user.email}
          </Typography>
        )}
      </Container>
    </Box>
  );
};

export default HomePage;
