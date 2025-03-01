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
} from '@mui/material';
import { Link, useNavigate } from 'react-router-dom';
import SearchIcon from '@mui/icons-material/Search';
import { useAuth } from '../../contexts/authTypes';
import axios from 'axios';
import YouTubePlaylistViewer from '../../components/YouTubePlaylistViewer/YouTubePlaylistViewer';
import './HomePage.scss';

const HomePage: React.FC = () => {
  const { user } = useAuth();
  const [songName, setSongName] = useState('');
  const [artistName, setArtistName] = useState('');
  const [error, setError] = useState('');
  const navigate = useNavigate();

  const handleSearch = async () => {
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

    // Navigate to SongPlayerPage with songName and artistName as query params
    navigate(`/play-song?song=${encodeURIComponent(songName)}&artist=${encodeURIComponent(artistName)}`);
  };

  return (
    <Box className="home-page">
      <Container maxWidth="md">
        <Typography variant="h2" className="welcome-message">
          Welcome {user ? user.name : 'to Chord KTV'}!
        </Typography>

        {!user && (
          <Alert severity="info" className="info-alert">
            Sign in to save your search history and favorites!
          </Alert>
        )}

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
          <Typography variant="body1" className="section-subtitle">
            Enter a song name and artist to get started.
          </Typography>
          <Stack direction="row" spacing={2} alignItems="center">
            <TextField
              label="Song Name"
              variant="filled"
              value={songName}
              onChange={(e) => setSongName(e.target.value)}
              fullWidth
              className="search-input"
            />
            <TextField
              label="Artist Name"
              variant="filled"
              value={artistName}
              onChange={(e) => setArtistName(e.target.value)}
              fullWidth
              className="search-input"
            />
            <IconButton
              component={Link}
              to="/play-song"
              aria-label="search"
              onClick={handleSearch}
              className="search-button"
            >
              <SearchIcon />
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
          <Typography variant="body1" className="section-subtitle">
            Select songs to play from your playlist.
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
