import React from 'react';
import { Box, Typography, TextField, IconButton } from '@mui/material';
import { Link } from 'react-router-dom';
import SearchIcon from '@mui/icons-material/Search';

const HomePage: React.FC = () => {
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
        Welcome to Chord KTV!
      </Typography>

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
          sx={{ mr: 2, flexGrow: 1, backgroundColor: 'white', borderRadius: 1  }}

        />
        <TextField
          label="Artist Name"
          variant="outlined"
          sx={{ flexGrow: 1, backgroundColor: 'white', borderRadius: 1  }}
        />
        <IconButton
          component={Link}
          to="/play-song"
          sx={{ p: '10px' }}
          aria-label="search"
        >
          <SearchIcon sx={{ color: 'white' }} />
        </IconButton>
      </Box>
    </Box>
  );
};

export default HomePage;
