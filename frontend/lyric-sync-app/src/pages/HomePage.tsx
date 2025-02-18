import React, { useState } from 'react';
import { Box, Typography, TextField, IconButton, Alert } from '@mui/material';
import { Link } from 'react-router-dom';
import SearchIcon from '@mui/icons-material/Search';
import { useAuth } from '../contexts/authTypes';
import axios from 'axios';

const HomePage: React.FC = () => {
  const { user } = useAuth();
  const [songName, setSongName] = useState('');
  const [artistName, setArtistName] = useState('');
  const [error, setError] = useState('');

  const handleSearch = async () => {
    if (user) {
      try {
        // Skeleton code to send the ID token in the Authorization header
        // Uncomment this to see how the Auth Bearer looks like in the request header of your browser's Network tab.
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
        setError(''); // Clear any existing error
      } catch {
        setError('Failed to save search history. Please try again.');
      }
    }
  };

  return (
    <Box
      sx={{
        display: 'flex',
        flexDirection: 'column',
        justifyContent: 'center',
        alignItems: 'center',
        minHeight: '100vh',
        backgroundColor: '#8a2be2',
        color: 'white',
      }}
    >
      <Typography variant="h2" sx={{ mb: 4 }}>
        Welcome {user ? user.name : 'to Chord KTV'}!
      </Typography>

      {!user && (
        <Alert severity="info" sx={{ mb: 2, backgroundColor: 'rgba(255,255,255,0.9)' }}>
          Sign in to save your search history and favorites!
        </Alert>
      )}

      {error && <Alert severity="error" sx={{ mb: 2 }}>{error}</Alert>}

      <Box
        sx={{
          display: 'flex',
          alignItems: 'center',
          mb: 4,
          width: '80%',
          maxWidth: '600px',
          borderRadius: 2,
          //   backgroundColor: 'white', // Set background to white
          padding: 2,
        }}
      >
        <TextField
          label="Song Name"
          variant="outlined"
          value={songName}
          onChange={(e) => setSongName(e.target.value)}
          sx={{ mr: 2, flexGrow: 1, backgroundColor: 'white', borderRadius: 1 }}
        />
        <TextField 
          label="Artist Name" 
          variant="outlined"
          value={artistName}
          onChange={(e) => setArtistName(e.target.value)}
          sx={{ flexGrow: 1, backgroundColor: 'white', borderRadius: 1 }}
        />
        <IconButton
          component={Link}
          to="/play-song"
          sx={{ p: '10px' }}
          aria-label="search"
          onClick={handleSearch}
        >
          <SearchIcon sx={{ color: 'white' }} />
        </IconButton>
      </Box>

      {user && (
        <Typography variant="body1" sx={{ mt: 2, opacity: 0.8 }}>
          Logged in as: {user.email}
        </Typography>
      )}
    </Box>
  );
};

export default HomePage;
