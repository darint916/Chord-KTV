import { useState } from 'react';
import {
  TextField,
  Button,
  Typography,
  CircularProgress,
  Paper,
  Box,
} from '@mui/material';
import { DataGrid, GridToolbarContainer } from '@mui/x-data-grid';
import { SongApi, Configuration } from '../api';
import '../styles/components/YouTubePlaylistViewer.scss';

// Define the type for a song
interface Song {
  id: number;
  title?: string | null;
  artist?: string | null;
  url?: string | null;
  duration?: string;
}

// Initialize API client
const songApi = new SongApi(
  new Configuration({
    basePath: import.meta.env.VITE_API_URL || 'http://localhost:5259',
  })
);

const YouTubePlaylistViewer = () => {
  const [playlistUrl, setPlaylistUrl] = useState('');
  const [songs, setSongs] = useState<Song[]>([]);
  const [playlistTitle, setPlaylistTitle] = useState('');
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState('');

  const extractPlaylistId = (url: string) => {
    const match = url.match(/[?&]list=([a-zA-Z0-9_-]+)/);
    return match ? match[1] : null;
  };

  const fetchPlaylistSongs = async () => {
    setError('');
    setSongs([]);
    setPlaylistTitle('');
    setLoading(true);

    const playlistId = extractPlaylistId(playlistUrl);
    if (!playlistId) {
      setError('Invalid playlist URL');
      setLoading(false);
      return;
    }

    try {
      const response = await songApi.apiYoutubePlaylistsPlaylistIdGet({
        playlistId: playlistId,
      });

      // Handle case where `response.videos` is null or undefined
      const videos = response.videos || [];
      setPlaylistTitle(response.playlistTitle || 'Untitled Playlist');
      setSongs(videos.map((song, index) => ({ ...song, id: index })));
    } catch {
      setError('Failed to fetch playlist. Please try again.');
    }
    setLoading(false);
  };

  // Define columns for the DataGrid
  const columns = [
    { field: 'title', headerName: 'Title', flex: 2 },
    { field: 'artist', headerName: 'Artist', flex: 1 },
    { field: 'duration', headerName: 'Duration', flex: 1 },
  ];

  // Custom toolbar component with playlist title
  const CustomToolbar = () => (
    <GridToolbarContainer>
      <Box className="toolbar-title">
        <Typography variant="h6">{playlistTitle}</Typography>
      </Box>
    </GridToolbarContainer>
  );

  return (
    <div className="youtube-playlist-viewer">
      <TextField
        fullWidth
        label="Enter YouTube Playlist URL"
        variant="filled"
        value={playlistUrl}
        onChange={(e) => setPlaylistUrl(e.target.value)}
        className="playlist-url-input"
      />
      <Button variant="contained" color="primary" onClick={fetchPlaylistSongs}>
        Load Playlist
      </Button>
      {loading && <CircularProgress className="loading-spinner" />}
      {error && <Typography className="error-message">{error}</Typography>}
      {songs.length > 0 && (
        <Paper className="data-grid-container">
          <DataGrid
            rows={songs}
            columns={columns}
            initialState={{
              pagination: {
                paginationModel: { pageSize: 5, page: 0 },
              },
            }}
            pageSizeOptions={[5, 10, 20]}
            slots={{
              toolbar: CustomToolbar,
            }}
          />
        </Paper>
      )}
    </div>
  );
};

export default YouTubePlaylistViewer;
