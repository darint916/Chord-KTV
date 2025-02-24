import React from 'react';
import { AppBar, Toolbar, Typography, Avatar, Menu, MenuItem } from '@mui/material';
import { Link } from 'react-router-dom';
import { GoogleLogin, CredentialResponse } from '@react-oauth/google';
import { useAuth } from '../contexts/authTypes';
import { jwtDecode } from 'jwt-decode';
import axios from 'axios';

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
        const response = await axios.post('http://localhost:5259/api/auth/google', null, {
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
    } catch (error) {
        console.error('Failed to authenticate with backend:', error);
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
    <AppBar position="static" elevation={0} style={{ backgroundColor: '#8a2be2' }}>
      <Toolbar>
        <Typography variant="h6" style={{ flexGrow: 1 }}>
          <Link to="/" style={{ textDecoration: 'none', color: 'white' }}>
            Chord KTV
          </Link>
        </Typography>
        
        {user ? (
          <>
            <Avatar
              src={user.picture}
              alt={user.name}
              onClick={handleMenuClick}
              style={{ cursor: 'pointer' }}
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