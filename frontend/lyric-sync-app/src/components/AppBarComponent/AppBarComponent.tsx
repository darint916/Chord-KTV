import React from 'react';
import { AppBar, Toolbar, Typography, Avatar, Menu, MenuItem, Button } from '@mui/material';
import { Link } from 'react-router-dom';
import { GoogleLogin, CredentialResponse } from '@react-oauth/google';
import { useAuth } from '../../contexts/authTypes';
import { jwtDecode } from 'jwt-decode';
<<<<<<< HEAD:frontend/lyric-sync-app/src/components/AppBarComponent.tsx
import axios from 'axios';
=======
import './AppBarComponent.scss';
>>>>>>> main:frontend/lyric-sync-app/src/components/AppBarComponent/AppBarComponent.tsx

interface GooglePayload {
  sub: string;
  name: string;
  email: string;
  picture: string;
}

const AppBarComponent: React.FC = () => {
  const { user, setUser } = useAuth();
  const [anchorEl, setAnchorEl] = React.useState<null | HTMLElement>(null);

  const handleLogin = async (credentialResponse: CredentialResponse) => {
    const decoded: GooglePayload = jwtDecode(credentialResponse.credential ?? '');
    
    try {
      await axios.post('http://localhost:5259/api/auth/google', null, {
        headers: {
          'Authorization': `Bearer ${credentialResponse.credential}`
        }
      });

      setUser({
        id: decoded.sub,
        name: decoded.name,
        email: decoded.email,
        picture: decoded.picture,
        idToken: credentialResponse.credential ?? ''
      });
    } catch {
      handleLoginError();
    }
  };

  const handleLogout = () => {
    setUser(null);
    setAnchorEl(null);
    localStorage.removeItem('user');
  };

  const handleMenuClick = (event: React.MouseEvent<HTMLElement>) => {
    setAnchorEl(event.currentTarget);
  };

  const handleMenuClose = () => {
    setAnchorEl(null);
  };

  const handleLoginError = () => {
    // Handle login error silently or show a user-friendly message
    setUser(null);
  };

  return (
    <AppBar position="static" elevation={0} className="app-bar">
      <Toolbar className="toolbar">
        <Typography variant="h6" className="title">
          <Link to="/" className="title">
            Chord KTV
          </Link>
        </Typography>
        <Button component={Link} to="/canvas" color="inherit">
          Canvas
        </Button>
        {user ? (
          <>
            <Avatar
              src={user.picture}
              alt={user.name}
              onClick={handleMenuClick}
              className="avatar"
            />
            <Menu
              anchorEl={anchorEl}
              open={Boolean(anchorEl)}
              onClose={handleMenuClose}
            >
              <MenuItem>{user.name}</MenuItem>
              <MenuItem onClick={handleLogout}>Logout</MenuItem>
            </Menu>
          </>
        ) : (
          <GoogleLogin
            onSuccess={handleLogin}
            onError={handleLoginError}
          />
        )}
      </Toolbar>
    </AppBar>
  );
};

export default AppBarComponent;