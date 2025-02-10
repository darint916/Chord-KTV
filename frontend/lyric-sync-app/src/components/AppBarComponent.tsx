import React from 'react';
import { AppBar, Toolbar, Typography, Button, TextField, IconButton, Box } from '@mui/material';
import { Link } from 'react-router-dom';

const AppBarComponent: React.FC = () => {
  return (
    <AppBar position="static" elevation={0} style={{ backgroundColor: '#8a2be2' }}>
      <Toolbar>
        <Typography variant="h6" style={{ flexGrow: 1 }}>
          <Link to="/" style={{ textDecoration: 'none', color: 'white' }}>
            Chord KTV
          </Link>
        </Typography>
        
        {/* Sign-In Button */}
        <Button color="inherit" style={{ marginLeft: '20px' }}>
          Sign In
        </Button>
      </Toolbar>
    </AppBar>
  );
};

export default AppBarComponent;