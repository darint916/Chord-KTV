import React, { useState } from 'react';
import { AppBar, Toolbar, Avatar, Menu, MenuItem, Button, Dialog, DialogTitle, DialogContent, DialogActions, TextField, Box, Typography, IconButton } from '@mui/material';
import { Link } from 'react-router-dom';
import { useAuth } from '../../contexts/AuthTypes';
import CloseIcon from '@mui/icons-material/Close';
import BugReportIcon from '@mui/icons-material/BugReport';
import GoogleAuth from '../GoogleAuth/GoogleAuth';
import './AppBarComponent.scss';
import logo from '../../assets/chordktv.png';

const AppBarComponent: React.FC = () => {
  const { user, setUser } = useAuth();
  const [anchorEl, setAnchorEl] = React.useState<null | HTMLElement>(null);
  const [bugMenuAnchorEl, setBugMenuAnchorEl] = React.useState<null | HTMLElement>(null);
  const [modalOpen, setModalOpen] = useState(false);
  const [issueType, setIssueType] = useState('');
  const [issueTitle, setIssueTitle] = useState('');

  const repoOwner = 'darint916';
  const repoName = 'Chord-KTV';

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

  const handleBugMenuClick = (event: React.MouseEvent<HTMLElement>) => {
    setBugMenuAnchorEl(event.currentTarget);
  };

  const handleBugMenuClose = () => {
    setBugMenuAnchorEl(null);
  };

  const openIssueModal = (type: string) => {
    setIssueType(type);
    setModalOpen(true);
    handleBugMenuClose();
  };

  const handleModalClose = () => {
    setModalOpen(false);
  };

  const handleOpenGitHubIssue = () => {
    // Base URL for GitHub issue creation
    const baseUrl = `https://github.com/${repoOwner}/${repoName}/issues/new`;

    // Get the appropriate template for the issue type
    let template = '';
    let labels = '';

    switch (issueType) {
    case 'bug':
      template = 'bug-report.md';
      labels = 'bug';
      break;
    case 'feature':
      template = 'feature-request.md';
      labels = 'enhancement';
      break;
    case 'lyrics':
      template = 'search-play-report.md';
      labels = 'search issue,bug';
      break;
    default:
      template = '';
    }

    const encodedTitle = encodeURIComponent(issueTitle || `New ${issueType} report`);

    let url = `${baseUrl}?`;
    if (template) {
      url += `template=${template}&`;
    }
    url += `title=${encodedTitle}&labels=${labels}`;
    window.open(url, '_blank');
    setModalOpen(false);
    setIssueTitle('');
  };

  return (
    <>
      <AppBar position="static" elevation={0} className="app-bar">
        <Toolbar className="toolbar">
          <div className="section">
            <Link to="/"><img src={logo} alt="Logo" className="logo" /></Link>
            <Link to="/" className="title">
              Chord KTV
            </Link>
          </div>
          <div className="section">
            <Button
              color="inherit"
              startIcon={<BugReportIcon />}
              onClick={handleBugMenuClick}
              className='report-button'
            >
              Report Issue
            </Button>
            <Menu
              anchorEl={bugMenuAnchorEl}
              open={Boolean(bugMenuAnchorEl)}
              onClose={handleBugMenuClose}
            >
              <MenuItem onClick={() => openIssueModal('song')}>Report Search/Play Related Issue</MenuItem>
              <MenuItem onClick={() => openIssueModal('bug')}>Report Misc Bug</MenuItem>
              <MenuItem onClick={() => openIssueModal('feature')}>Request Feature</MenuItem>
              <MenuItem onClick={() => openIssueModal('lyrics')}>Report Other</MenuItem>
            </Menu>

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

      <Dialog
        open={modalOpen}
        onClose={handleModalClose}
        maxWidth="sm"
        fullWidth
      >
        <DialogTitle>
          <Box display="flex" justifyContent="space-between" alignItems="center">
            <Typography variant="h6">
              {issueType === 'bug' ? 'Report a Bug' :
                issueType === 'feature' ? 'Request a Feature' :
                  'Report Lyrics Issue'}
            </Typography>
            <IconButton onClick={handleModalClose}>
              <CloseIcon />
            </IconButton>
          </Box>
        </DialogTitle>
        <DialogContent>
          <Typography variant="body2" sx={{ mb: 2 }}>
            You'll be redirected to GitHub to create an issue with the appropriate template (title copied over).
            Please enter a brief title for your issue:
          </Typography>
          <TextField
            autoFocus
            margin="dense"
            label="Issue Title"
            fullWidth
            value={issueTitle}
            onChange={(e) => setIssueTitle(e.target.value)}
            variant="outlined"
          />
        </DialogContent>
        <DialogActions>
          <Button onClick={handleModalClose}>Cancel</Button>
          <Button
            onClick={handleOpenGitHubIssue}
            variant="contained"
            color="primary"
          >
            Continue to GitHub
          </Button>
        </DialogActions>
      </Dialog>
    </>
  );
};

export default AppBarComponent;
