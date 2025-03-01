import React, { useState, useEffect, useRef } from 'react';
import { Container, Stack, Typography } from '@mui/material';
import YouTubePlayer from '../../components/YouTubePlayer/YouTubePlayer';
import LyricDisplay from '../../components/LyricDisplay/LyricDisplay';
import Controls from '../../components/Controls/Controls';
import axios from 'axios';
import './SongPlayerPage.scss';
import Grid from '@mui/material/Grid2';
import { useSong } from '../../contexts/SongContext';

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
  const [rawLrcLyrics, setRawLrcLyrics] = useState<string>('');
  const [currentTime, setCurrentTime] = useState<number>(0);
  const [isPlaying, setIsPlaying] = useState<boolean>(false);
  const [duration, setDuration] = useState<number>(100);
  const videoId = 'ChQaa0eqZak';
  const playerRef = useRef<YouTubePlayer | null>(null);
  const { songName, artistName } = useSong();

  useEffect(() => {
    const fetchLyrics = async () => {
      try {
        const response = await axios.get('https://lrclib.net/api/get', {
          params: {
            track_name: songName,
            artist_name: artistName,
          },
        });
        setRawLrcLyrics(response.data.syncedLyrics);
      } catch (err) {
        console.error('Error fetching lyrics:', err); // eslint-disable-line no-console
      }
    };
    fetchLyrics();
  }, [songName, artistName]);

  const handlePlayerReady = (playerInstance: YouTubePlayer) => {
    playerRef.current = playerInstance;
    if (playerInstance.getDuration) { setDuration(playerInstance.getDuration()); }


    setInterval(() => {
      if (playerRef.current) { setCurrentTime(playerRef.current.getCurrentTime()); }
    }, 500);
  };

  const handlePlayPause = (playState: boolean) => setIsPlaying(playState);
  const handleSeek = (seekTime: number) => setCurrentTime(seekTime);

  return (
    <div className="song-player-page">
      <Container maxWidth="lg" className="song-player-container">
        <Grid container spacing={4} className="song-player-content">
          {/* we use grid now as later plan to add additional column additions, change spacing if needed*/}
          <Grid size={12}>
            <Stack spacing={3} className="stack">
              <Typography variant="h3" className="song-title">
                {songName} by {artistName}
              </Typography>
              <YouTubePlayer videoId={videoId} onReady={handlePlayerReady} />
              <LyricDisplay rawLrcLyrics={rawLrcLyrics} currentTime={currentTime} isPlaying={isPlaying}/>
              <Controls
                playerRef={playerRef}
                currentTime={currentTime}
                duration={duration}
                onSeek={handleSeek}
                onPlayPause={handlePlayPause}
                isPlaying={isPlaying}
              />
            </Stack>
          </Grid>
        </Grid>
      </Container>
    </div>
  );
};

export default SongPlayerPage;
