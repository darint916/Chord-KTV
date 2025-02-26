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
import { Link } from 'react-router-dom';
import SearchIcon from '@mui/icons-material/Search';
import { useAuth } from '../contexts/authTypes';
import axios from 'axios';
import YouTubePlaylistViewer from '../components/YouTubePlaylistViewer';

const HomePage: React.FC = () => {
  const { user } = useAuth();
  const [songName, setSongName] = useState('');
  const [artistName, setArtistName] = useState('');
  const [error, setError] = useState('');

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
  };

  return (
    <Box
      sx={{
        minHeight: '100vh',
        backgroundColor: '#8a2be2',
        color: 'white',
        padding: 4,
      }}
    >
      <Container maxWidth="md">
        <Typography variant="h2" sx={{ mb: 4, textAlign: 'center', fontWeight: 'bold' }}>
          Welcome {user ? user.name : 'to Chord KTV'}!
        </Typography>

        {!user && (
          <Alert
            severity="info"
            sx={{ mb: 4, backgroundColor: 'rgba(255,255,255,0.9)', color: 'black' }}
          >
            Sign in to save your search history and favorites!
          </Alert>
        )}

        {error && (
          <Alert severity="error" sx={{ mb: 4 }}>
            {error}
          </Alert>
        )}

        {/* Search Section */}
        <Paper
          elevation={3}
          sx={{
            padding: 3,
            mb: 4,
            backgroundColor: 'rgba(255,255,255,0.9)',
            borderRadius: 2,
          }}
        >
          <Typography variant="h5" sx={{ mb: 2, color: '#8a2be2' }}>
            Search for a Song
          </Typography>
          <Typography variant="body1" sx={{ mb: 3, color: 'text.secondary' }}>
            Enter a song name and artist to get started.
          </Typography>
          <Stack direction="row" spacing={2} alignItems="center">
            <TextField
              label="Song Name"
              variant="outlined"
              value={songName}
              onChange={(e) => setSongName(e.target.value)}
              fullWidth
              sx={{ backgroundColor: 'white', borderRadius: 1 }}
            />
            <TextField
              label="Artist Name"
              variant="outlined"
              value={artistName}
              onChange={(e) => setArtistName(e.target.value)}
              fullWidth
              sx={{ backgroundColor: 'white', borderRadius: 1 }}
            />
            <IconButton
              component={Link}
              to="/play-song"
              aria-label="search"
              onClick={handleSearch}
              sx={{ backgroundColor: '#8a2be2', color: 'white', '&:hover': { backgroundColor: '#7b1fa2' } }}
            >
              <SearchIcon />
            </IconButton>
          </Stack>
        </Paper>

        {/* OR Divider */}
        <Box
          sx={{
            display: 'flex',
            justifyContent: 'center',
            alignItems: 'center',
            my: 4,
          }}
        >
          <Paper
            elevation={3}
            sx={{
              padding: '8px 16px',
              backgroundColor: 'rgba(255,255,255,0.9)',
              borderRadius: 20,
            }}
          >
            <Typography variant="body1" sx={{ color: '#8a2be2', fontWeight: 'bold' }}>
              OR
            </Typography>
          </Paper>
        </Box>

        {/* YouTube Playlist Section */}
        <Paper
          elevation={3}
          sx={{
            padding: 3,
            backgroundColor: 'rgba(255,255,255,0.9)',
            borderRadius: 2,
          }}
        >
          <Typography variant="h5" sx={{ mb: 2, color: '#8a2be2' }}>
            Load a YouTube Playlist
          </Typography>
          <Typography variant="body1" sx={{ mb: 3, color: 'text.secondary' }}>
            Select songs to play from your playlist.
          </Typography>
          <YouTubePlaylistViewer />
        </Paper>

        {user && (
          <Typography variant="body1" sx={{ mt: 4, textAlign: 'center', opacity: 0.8 }}>
            Logged in as: {user.email}
          </Typography>
        )}
      </Container>
    </Box>
  );
};

export default HomePage;
