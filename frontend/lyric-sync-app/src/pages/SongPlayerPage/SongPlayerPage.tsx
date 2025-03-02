import React, { useState, useEffect, useRef } from 'react';
import { Container, Stack, Typography } from '@mui/material';
import axios from 'axios';
import YouTubePlayer from '../../components/YouTubePlayer/YouTubePlayer';
import LyricDisplay from '../../components/LyricDisplay/LyricDisplay';
import Controls from '../../components/Controls/Controls';
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
  const [currentTime, setCurrentTime] = useState<number>(0);
  const [isPlaying, setIsPlaying] = useState<boolean>(false);
  const [duration, setDuration] = useState<number>(100);
  const playerRef = useRef<YouTubePlayer | null>(null);
  const [videoId, setVideoId] = useState<string | null>(null);
  const { song } = useSong();
  if (!song) {
    return <Typography variant="h5">Error: No song selected</Typography>;
  }
  
  if (!song.lrcLyrics || !song.lrcLyrics.trim()) {
    return <Typography variant="h5">Error: No time-synced lyrics found for song</Typography>;
  }

  // Fetch YouTube video ID if song.youtubeUrl undefined
  // TODO: Delete this hook and replace with backend stub once implemented
  useEffect(() => {
    if (song.youTubeUrl) {
      const match = song.youTubeUrl.match(/[?&]v=([a-zA-Z0-9_-]+)/);
      if (match) {
        setVideoId(match[1]);
        return;
      }
    }

    const fetchVideoId = async () => {
      try {
        const response = await axios.get('https://www.googleapis.com/youtube/v3/search', {
          params: {
            part: 'snippet',
            q: `${song.title} ${song.artist} official music video`,
            key: import.meta.env.VITE_YT_API_KEY,
            maxResults: 1,
            type: 'video',
          },
        });

        const items = response.data.items;
        if (items.length > 0) {
          setVideoId(items[0].id.videoId);
        } else {
          return <Typography variant="h5">Error: No YouTube video found for song</Typography>;
        }
      } catch {
        return <Typography variant="h5">Error: YouTube video fetch failed</Typography>;
      }
    };

    fetchVideoId();
  }, [song.title, song.artist, song.youTubeUrl]);

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
                {song.title} by {song.artist}
              </Typography>
              <YouTubePlayer videoId={videoId ?? ''} onReady={handlePlayerReady} />
              <LyricDisplay rawLrcLyrics={song.lrcLyrics} currentTime={currentTime} isPlaying={isPlaying}/>
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
