import { useState, useEffect } from 'react';
import { Typography, CircularProgress, Alert, Box, Button } from '@mui/material';
import { songApi } from '../../api/apiClient';
import { useSong } from '../../contexts/SongContext';
import { useNavigate } from 'react-router-dom';
import './YouTubePlaylistLoader.scss';
import { v4 as uuidv4 } from 'uuid';

interface YouTubePlaylistLoaderProps {
  playlistUrl: string;
  onRetry?: () => void;
}

const YouTubePlaylistLoader: React.FC<YouTubePlaylistLoaderProps> = ({ 
  playlistUrl,
  onRetry 
}) => {
  const { setQueue, setCurrentPlayingId, setSong } = useSong();
  const navigate = useNavigate();
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);

  const extractPlaylistId = (url: string): string | null => {
    if (!url) return null;
    const match = url.match(/[?&]list=([a-zA-Z0-9_-]+)/);
    return match ? match[1] : null;
  };

  const loadPlaylist = async () => {
    setLoading(true);
    setError(null);

    try {
      const playlistId = extractPlaylistId(playlistUrl);
      if (!playlistId) throw new Error('Invalid YouTube playlist URL');

      const response = await songApi.apiYoutubePlaylistsPlaylistIdGet({ playlistId });
      const videos = response.videos || [];
      if (videos.length === 0) throw new Error('This playlist contains no videos');

      // Create queue items with basic info
      const newQueue = videos.map(video => ({
        queueId: uuidv4(),
        title: video.title || 'Unknown Track',
        artist: video.artist || 'Unknown Artist',
        youTubeId: extractVideoId(video.url ?? "") || '',
        lyrics: "",
        apiRequested: false
      }));

      // Set the queue with new songs
      setQueue(newQueue);
      
      // Immediately process the first song
      const firstSong = newQueue[0];
      try {
        const processed = await songApi.apiSongsSearchPost({
          fullSongRequestDto: {
            title: firstSong.title,
            artist: firstSong.artist,
            youTubeId: firstSong.youTubeId,
            lyrics: firstSong.lyrics
          }
        });
        
        // Update queue with processed data for first song
        setQueue(prev => prev.map(item => 
          item.queueId === firstSong.queueId 
            ? { ...item, processedData: processed, apiRequested: true }
            : item
        ));
        
        // Set as current song with full data
        setCurrentPlayingId(firstSong.queueId);
        setSong(processed);
      } catch (err) {
        console.error('Failed to process first song:', err);
        // Fallback to basic info if processing fails
        setCurrentPlayingId(firstSong.queueId);
        setSong({
          title: firstSong.title,
          artist: firstSong.artist,
          youTubeId: firstSong.youTubeId,
          lrcLyrics: '',
          lrcRomanizedLyrics: '',
          lrcTranslatedLyrics: ''
        });
      }
      
      navigate('/play-song');
    } catch (err) {
      setError(err instanceof Error ? err.message : 'Failed to load playlist');
    } finally {
      setLoading(false);
    }
  };

  const extractVideoId = (url: string): string | null => {
    if (!url) return null;
    const match = url.match(/(?:\?v=|\/embed\/|\.be\/|\/watch\?v=|\/watch\?.+&v=)([a-zA-Z0-9_-]{11})/);
    return match ? match[1] : null;
  };

  useEffect(() => {
    loadPlaylist();
  }, [playlistUrl]);

  const handleRetry = () => {
    if (onRetry) {
      onRetry();
    } else {
      loadPlaylist();
    }
  };

  if (loading) {
    return (
      <Box display="flex" justifyContent="center" alignItems="center" minHeight="100px">
        <CircularProgress />
        <Typography variant="body1" ml={2}>Loading playlist...</Typography>
      </Box>
    );
  }

  if (error) {
    return (
      <Box>
        <Alert severity="error" sx={{ mb: 2 }}>
          {error}
        </Alert>
        <Button 
          variant="contained" 
          onClick={handleRetry}
          color="primary"
        >
          Try Again
        </Button>
      </Box>
    );
  }

  return null;
};

export default YouTubePlaylistLoader;
