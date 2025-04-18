import { useState, useEffect } from 'react';
import { Typography, CircularProgress, Alert, Box } from '@mui/material';
import { songApi } from '../../api/apiClient';
import { useSong } from '../../contexts/SongContext';
import { useNavigate } from 'react-router-dom';
import './YouTubePlaylistLoader.scss';
import { v4 as uuidv4 } from 'uuid';

interface YouTubePlaylistLoaderProps {
  playlistUrl: string;
}

const YouTubePlaylistLoader: React.FC<YouTubePlaylistLoaderProps> = ({ playlistUrl }) => {
  const { setQueue, setCurrentPlayingId } = useSong();
  const navigate = useNavigate();
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState('');

  const extractPlaylistId = (url: string) => {
    const match = url.match(/[?&]list=([a-zA-Z0-9_-]+)/);
    return match ? match[1] : null;
  };

  useEffect(() => {
    const loadPlaylist = async () => {
      setLoading(true);
      try {
        const playlistId = extractPlaylistId(playlistUrl);
        const response = await songApi.apiYoutubePlaylistsPlaylistIdGet({
          playlistId: playlistId!,
        });

        const videos = response.videos || [];
        if (videos.length === 0) {
          setError('Playlist is empty');
          return;
        }

        // Add all songs to queue with basic info
        const newQueue = videos.map(video => ({
          queueId: uuidv4(),
          title: video.title || 'Unknown',
          artist: video.artist || 'Unknown',
          youtubeUrl: video.url || '',
          lyrics: "",
          apiRequested: false
        }));

        setQueue(newQueue);
        setCurrentPlayingId(newQueue[0].queueId);
        navigate('/play-song');
      } finally {
        setLoading(false);
      }
    };

    loadPlaylist();
  }, [playlistUrl]);

  return loading ? <CircularProgress /> : null;
};

export default YouTubePlaylistLoader;
