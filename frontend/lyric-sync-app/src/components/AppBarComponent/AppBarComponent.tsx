import React from 'react';
import { AppBar, Toolbar, Typography, Avatar, Menu, MenuItem, Button } from '@mui/material';
import { Link } from 'react-router-dom';
import { useAuth } from '../../contexts/AuthTypes';
import GoogleAuth from '../GoogleAuth/GoogleAuth';
import './AppBarComponent.scss';

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
          <GoogleAuth />
        )}
      </Toolbar>
    </AppBar>
  );
};

export default AppBarComponent;