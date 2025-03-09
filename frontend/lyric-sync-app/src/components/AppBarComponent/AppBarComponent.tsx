import React from 'react';
import { AppBar, Toolbar, Avatar, Menu, MenuItem, Button } from '@mui/material';
import { Link } from 'react-router-dom';
import { GoogleLogin, CredentialResponse } from '@react-oauth/google';
import { useAuth } from '../../contexts/authTypes';
import { jwtDecode } from 'jwt-decode';
import './AppBarComponent.scss';
import logo from '../../assets/chordktv.png';

interface GooglePayload {
  sub: string;
  name: string;
  email: string;
  picture: string;
}

const AppBarComponent: React.FC = () => {
  const { user, setUser } = useAuth();
  const [anchorEl, setAnchorEl] = React.useState<null | HTMLElement>(null);

  const handleLogin = (credentialResponse: CredentialResponse) => {
    const decoded: GooglePayload = jwtDecode(credentialResponse.credential ?? '');
    setUser({
      id: decoded.sub,
      name: decoded.name,
      email: decoded.email,
      picture: decoded.picture,
      idToken: credentialResponse.credential ?? ''
    });
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
        <div className="section">
          <Link to="/"><img src={logo} alt="Logo" className="logo" /></Link>
          <Link to="/" className="title">
            Chord KTV
          </Link>
        </div>
        <div className="section">
          <Button component={Link} to="/canvas" className="button-styles">
            Handwriting Demo
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
        </div>
      </Toolbar>
    </AppBar>
  );
};

export default AppBarComponent;