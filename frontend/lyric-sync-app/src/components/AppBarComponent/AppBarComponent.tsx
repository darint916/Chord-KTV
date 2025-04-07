import React from 'react';
import { AppBar, Toolbar, Avatar, Menu, MenuItem } from '@mui/material';
import { Link } from 'react-router-dom';
import { useAuth } from '../../contexts/AuthTypes';
import GoogleAuth from '../GoogleAuth/GoogleAuth';
import './AppBarComponent.scss';
import logo from '../../assets/chordktv.png';

const AppBarComponent: React.FC = () => {
  const { user, setUser } = useAuth();
  const [anchorEl, setAnchorEl] = React.useState<null | HTMLElement>(null);

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
            <GoogleAuth />
          )}
        </div>
      </Toolbar>
    </AppBar>
  );
};

export default AppBarComponent;