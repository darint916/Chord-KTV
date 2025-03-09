import { useState, useEffect } from 'react';
import {
  Typography,
  Paper,
  Box,
} from '@mui/material';
import { DataGrid, GridRowParams, GridToolbarContainer } from '@mui/x-data-grid';
import { songApi } from '../../api/apiClient';
import { useSong } from '../../contexts/SongContext';
import { useNavigate } from 'react-router-dom';
import './YouTubePlaylistViewer.scss';

// Define the type for a song
interface Song {
  id: number;
  title?: string | null;
  artist?: string | null;
  url?: string | null;
  duration?: string;
}

interface YouTubePlaylistViewerProps {
  playlistUrl: string;
}

const YouTubePlaylistViewer: React.FC<YouTubePlaylistViewerProps> = ({ playlistUrl }) => {
  const [songs, setSongs] = useState<Song[]>([]);
  const [playlistTitle, setPlaylistTitle] = useState('');
  const [searchLoading, setSearchLoading] = useState(false);
  const [error, setError] = useState('');
  const { setSong } = useSong();
  const navigate = useNavigate();

  const extractPlaylistId = (url: string) => {
    const match = url.match(/[?&]list=([a-zA-Z0-9_-]+)/);
    return match ? match[1] : null;
  };

  useEffect(() => {
    if (playlistUrl) {
      fetchPlaylistSongs();
    }
  }, [playlistUrl]);

  const fetchPlaylistSongs = async () => {
    setError('');
    setSongs([]);
    setPlaylistTitle('');

    const playlistId = extractPlaylistId(playlistUrl);
    if (!playlistId) {
      setError('Invalid playlist URL');
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
  };

  const handleRowClick = async (params: GridRowParams) => {
    setSearchLoading(true);
    try {
      const response = await songApi.apiSongsSearchPost({
        fullSongRequestDto: {
          title: params.row.title,
          artist: params.row.artist,
          youTubeId: params.row.url
        }
      });
      setSong(response);
      navigate('/play-song', { state: { playlistUrl } });
    } catch {
      setError('Search failed. Please try again.');
    } finally {
      setSearchLoading(false);
    }
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
        <Typography variant="h5">{playlistTitle}</Typography>
      </Box>
    </GridToolbarContainer>
  );

  return (
    <div className="youtube-playlist-viewer">
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
            onRowClick={!searchLoading ? handleRowClick : undefined}
            loading={searchLoading}
            slotProps={{
              loadingOverlay: {
                variant: 'linear-progress',
                noRowsVariant: 'skeleton',
              },
            }}
          />
        </Paper>
      )}
    </div>
  );
};

export default YouTubePlaylistViewer;
