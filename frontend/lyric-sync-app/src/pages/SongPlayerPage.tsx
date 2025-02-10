import React, { useState, useEffect, useRef } from 'react';
import { Container, Typography, Box, Grid, AppBar, Toolbar, Button } from '@mui/material';
import YouTubePlayer from '../components/YouTubePlayer';
import LyricDisplay from '../components/LyricDisplay';
import Controls from '../components/Controls';
import axios from 'axios';

const SongPlayerPage: React.FC = () => {
  const [lyrics, setLyrics] = useState<{ time: number; text: string }[]>([]);
  const [currentTime, setCurrentTime] = useState<number>(0);
  const [isPlaying, setIsPlaying] = useState<boolean>(false);
  const [duration, setDuration] = useState<number>(100);
  const videoId = 'ChQaa0eqZak';
  const playerRef = useRef<any>(null);

  useEffect(() => {
    // Fetch LRC file and parse
    const fetchLyrics = async () => {
      try {
        const response = await axios.get('https://lrclib.net/api/get', {
          params: {
            track_name: 'midnight cruisin',
            artist_name: 'Kingo Hamada',
          },
        });
        const parsedLyrics = parseLRC(response.data.syncedLyrics);
        setLyrics(parsedLyrics);
      } catch (err) {
        console.error('Error fetching lyrics:', err);
      }
    };
    fetchLyrics();
  }, []);

  const parseLRC = (lrc: string) => {
    return lrc.split('\n').map((line) => {
      const match = line.match(/\[(\d+):(\d+\.\d+)](.+)/);
      if (!match) return null;

      const time = parseInt(match[1]) * 60 + parseFloat(match[2]);
      return { time, text: match[3] };
    }).filter(Boolean) as { time: number; text: string }[];
  };

  const handlePlayerReady = (playerInstance: any) => {
    playerRef.current = playerInstance;

    // Get duration when player is ready
    playerInstance.getDuration && setDuration(playerInstance.getDuration());

    const intervalId = setInterval(() => {
      if (playerRef.current) {
        setCurrentTime(playerRef.current.getCurrentTime());
      }
    }, 500); // Sync every 500ms

    return () => clearInterval(intervalId);
  };

  const handlePlayPause = (playState: boolean) => {
    setIsPlaying(playState);
  };

  const handleSeek = (seekTime: number) => {
    setCurrentTime(seekTime);
  };

  return (
    <div>
      {/* Main Content Container */}
      <Container maxWidth="lg" sx={{ marginTop: 5 }}>
        <Box display="flex" flexDirection="column" alignItems="center" justifyContent="center" textAlign="center">
          <Typography variant="h3" color="white" gutterBottom>
            Midnight Cruisin' by Kingo Hamada
          </Typography>

          <YouTubePlayer videoId={videoId} onReady={handlePlayerReady} />
          <LyricDisplay lyrics={lyrics} currentTime={currentTime} />
          <Controls
            playerRef={playerRef}
            currentTime={currentTime}
            duration={duration}
            onSeek={handleSeek}
            onPlayPause={handlePlayPause}
            isPlaying={isPlaying}
          />
        </Box>
      </Container>
    </div>
  );
};

export default SongPlayerPage;
