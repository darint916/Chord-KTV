import { useState } from "react";
import {
  TextField,
  Button,
  Typography,
  Container,
  CircularProgress,
  Paper,
} from "@mui/material";
import { DataGrid } from "@mui/x-data-grid";
import { SongApi, Configuration } from "../api";

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
    basePath: import.meta.env.VITE_API_URL || "http://localhost:5259",
  })
);

const YouTubePlaylistViewer = () => {
  const [playlistUrl, setPlaylistUrl] = useState("");
  const [songs, setSongs] = useState<Song[]>([]); // Explicitly define the type of `songs`
  const [playlistTitle, setPlaylistTitle] = useState("");
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState("");

  const extractPlaylistId = (url: string) => {
    const match = url.match(/[?&]list=([a-zA-Z0-9_-]+)/);
    return match ? match[1] : null;
  };

  const fetchPlaylistSongs = async () => {
    setError("");
    setSongs([]);
    setPlaylistTitle("");
    setLoading(true);

    const playlistId = extractPlaylistId(playlistUrl);
    if (!playlistId) {
      setError("Invalid playlist URL");
      setLoading(false);
      return;
    }

    try {
      const response = await songApi.apiYoutubePlaylistsPlaylistIdGet({
        playlistId: playlistId,
      });

      // Handle case where `response.videos` is null or undefined
      const videos = response.videos || [];
      setPlaylistTitle(response.playlistTitle || "Untitled Playlist");
      setSongs(videos.map((song, index) => ({ ...song, id: index })));
    } catch (err) {
      setError("Failed to fetch playlist. Please try again.");
    }
    setLoading(false);
  };

  // Define columns for the DataGrid
  const columns = [
    { field: "title", headerName: "Title", flex: 2 },
    { field: "artist", headerName: "Artist", flex: 1 },
    { field: "duration", headerName: "Duration", flex: 1 },
  ];

  return (
    <Container maxWidth="md" sx={{ mt: 4 }}>
      <Typography variant="h5" gutterBottom>
        YouTube Playlist Viewer
      </Typography>
      <TextField
        fullWidth
        label="Enter YouTube Playlist URL"
        variant="filled"
        value={playlistUrl}
        onChange={(e) => setPlaylistUrl(e.target.value)}
        sx={{ backgroundColor: 'white', mb: 2 }}
      />
      <Button variant="contained" color="primary" onClick={fetchPlaylistSongs}>
        Load Playlist
      </Button>
      {loading && <CircularProgress sx={{ mt: 2 }} />}
      {error && <Typography color="error" sx={{ mt: 2 }}>{error}</Typography>}
      {playlistTitle && (
        <Typography variant="h6" sx={{ mt: 2 }}>
          {playlistTitle}
        </Typography>
      )}
      {songs.length > 0 && ( // Conditionally render the DataGrid
        <Paper sx={{ mt: 2, height: 400, width: "100%" }}>
          <DataGrid
            rows={songs}
            columns={columns}
            initialState={{
              pagination: {
                paginationModel: { pageSize: 5, page: 0 }, // Set initial page size
              },
            }}
            pageSizeOptions={[5, 10, 20]} // Rows per page options
          />
        </Paper>
      )}
    </Container>
  );
};

export default YouTubePlaylistViewer;
