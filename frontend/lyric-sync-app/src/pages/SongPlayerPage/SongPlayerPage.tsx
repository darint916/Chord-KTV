import React, { useState, useEffect, useRef } from 'react';
import { Container, Typography, Box } from '@mui/material';
import YouTubePlayer from '../../components/YouTubePlayer/YouTubePlayer';
import LyricDisplay from '../../components/LyricDisplay/LyricDisplay';
import Controls from '../../components/Controls/Controls';
import axios from 'axios';
import './SongPlayerPage.scss';

// Define the YouTubePlayer interface
interface YouTubePlayer {
  seekTo: (_seconds: number, _allowSeekAhead: boolean) => void;
  playVideo: () => void;
  pauseVideo: () => void;
  getCurrentTime: () => number;
  getDuration: () => number;
  setVolume: (_volume: number) => void;
}

const SongPlayerPage: React.FC = () => {
  const [lyrics, setLyrics] = useState<{ time: number; text: string }[]>([]);
  const [currentTime, setCurrentTime] = useState<number>(0);
  const [isPlaying, setIsPlaying] = useState<boolean>(false);
  const [duration, setDuration] = useState<number>(100);
  const videoId = 'ChQaa0eqZak';
  const playerRef = useRef<YouTubePlayer | null>(null);

  useEffect(() => {
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
        console.error('Error fetching lyrics:', err); // eslint-disable-line no-console
      }
    };
    fetchLyrics();
  }, []);

  const parseLRC = (lrc: string) => {
    return lrc
      .split('\n')
      .map((line) => {
        const match = line.match(/\[(\d+):(\d+\.\d+)](.+)/);
        if (!match) {return null;}
        const time = parseInt(match[1]) * 60 + parseFloat(match[2]);
        return { time, text: match[3] };
      })
      .filter(Boolean) as { time: number; text: string }[];
  };

  const handlePlayerReady = (playerInstance: YouTubePlayer) => {
    playerRef.current = playerInstance;
    if (playerInstance.getDuration) {setDuration(playerInstance.getDuration());}

    const intervalId = setInterval(() => {
      if (playerRef.current) {setCurrentTime(playerRef.current.getCurrentTime());}
    }, 500);

    return () => clearInterval(intervalId);
  };

  const handlePlayPause = (playState: boolean) => setIsPlaying(playState);
  const handleSeek = (seekTime: number) => setCurrentTime(seekTime);

  return (
    <div className="song-player-page">
      <Container maxWidth="lg" className="song-player-container">
        <Box className="song-player-content">
          <Typography variant="h3" className="song-title">
            Midnight Cruisin&#39; by Kingo Hamada
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
