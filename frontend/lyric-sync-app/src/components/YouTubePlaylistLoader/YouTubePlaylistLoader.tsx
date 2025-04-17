import { useState, useEffect } from 'react';
import { Typography, CircularProgress, Alert, Box } from '@mui/material';
import { songApi } from '../../api/apiClient';
import { useSong } from '../../contexts/SongContext';
import { useNavigate } from 'react-router-dom';
import './YouTubePlaylistLoader.scss';

interface YouTubePlaylistLoaderProps {
  playlistUrl: string;
}

const YouTubePlaylistLoader: React.FC<YouTubePlaylistLoaderProps> = ({ playlistUrl }) => {
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState('');
  const { setSong, setQueue, setCurrentPlayingId } = useSong();
  const navigate = useNavigate();

  const extractPlaylistId = (url: string) => {
    const match = url.match(/[?&]list=([a-zA-Z0-9_-]+)/);
    return match ? match[1] : null;
  };

  const extractYouTubeVideoId = (url: string | null | undefined): string | null => {
    if (!url) return null;
    const match = url.match(/(?:\?v=|\/embed\/|\.be\/|\/watch\?v=|\/watch\?.+&v=)([a-zA-Z0-9_-]{11})/);
    return match ? match[1] : null;
  };

  useEffect(() => {
    const processPlaylist = async () => {
      setError('');
      setLoading(true);

      const playlistId = extractPlaylistId(playlistUrl);
      if (!playlistId) {
        setError('Invalid playlist URL');
        setLoading(false);
        return;
      }

      try {
        // Fetch playlist info
        const response = await songApi.apiYoutubePlaylistsPlaylistIdGet({
          playlistId: playlistId,
        });

        const videos = response.videos || [];
        if (videos.length === 0) {
          setError('Playlist is empty');
          return;
        }

        // Process all videos in parallel
        const songPromises = videos.map(async (video) => {
          const vidId = extractYouTubeVideoId(video.url);
          try {
            return await songApi.apiSongsSearchPost({
              fullSongRequestDto: {
                title: video.title,
                artist: video.artist,
                youTubeId: vidId || ''
              }
            });
          } catch (error) {
            console.error(`Failed to process song: ${video.title}`, error);
            return null;
          }
        });

        // Wait for all songs to be processed
        const songs = (await Promise.all(songPromises)).filter(Boolean);

        if (songs.length === 0) {
          setError('No valid songs found in playlist');
          return;
        }

        // Add all songs to queue with unique IDs
        const newQueueItems = songs.map(song => ({
          ...song,
          queueId: crypto.randomUUID() // or use uuidv4()
        }));

        // Update queue and set first song as current
        setQueue(newQueueItems);
        setCurrentPlayingId(newQueueItems[0].queueId);
        setSong(newQueueItems[0]);

        // Navigate to player page
        navigate('/play-song');
      } catch (error) {
        setError('Failed to process playlist. Please try again.');
        console.error(error);
      } finally {
        setLoading(false);
      }
    };

    processPlaylist();
  }, [playlistUrl, navigate, setQueue, setCurrentPlayingId, setSong]);

  return (
    <div className="youtube-playlist-viewer">
      {loading && (
        <Box display="flex" flexDirection="column" alignItems="center" gap={2}>
          <CircularProgress />
          <Typography>Processing playlist...</Typography>
        </Box>
      )}
      {error && <Alert severity="error">{error}</Alert>}
    </div>
  );
};

export default YouTubePlaylistLoader;
