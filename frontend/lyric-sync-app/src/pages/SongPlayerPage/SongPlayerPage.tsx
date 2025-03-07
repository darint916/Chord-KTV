import React, { useState, useRef } from 'react';
import { Container, Typography, Box } from '@mui/material';
import YouTubePlayer from '../../components/YouTubePlayer/YouTubePlayer';
import LyricDisplay from '../../components/LyricDisplay/LyricDisplay';
import './SongPlayerPage.scss';
import Grid from '@mui/material/Grid2';
import Tabs from '@mui/material/Tabs';
import Tab from '@mui/material/Tab';
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
  const [isPlaying] = useState<boolean>(false);
  const playerRef = useRef<YouTubePlayer | null>(null);
  const { song } = useSong();
  const [selectedTab, setSelectedTab] = useState(0);

  if (!song) {
    return <Typography variant="h5">Error: No song selected</Typography>;
  }
  
  if (!song.lrcLyrics || !song.lrcLyrics.trim()) {
    return <Typography variant="h5">Error: No time-synced lyrics found for song</Typography>;
  }

  if (!song.youTubeId || !song.youTubeId.trim()) {
    return <Typography variant="h5">Error: No YouTube video found for song</Typography>;
  }

  const handlePlayerReady = (playerInstance: YouTubePlayer) => {
    playerRef.current = playerInstance;

    setInterval(() => {
      if (playerRef.current) { setCurrentTime(playerRef.current.getCurrentTime()); }
    }, 1); // Noticed player was falling behind, 1ms for absolute accuracy
  };

  const handleTabChange = (_event: React.SyntheticEvent, newValue: number) => {
    setSelectedTab(newValue);
  };

  return (
    <div className="song-player-page">
      <Container maxWidth="lg" className="song-player-container">
        <Typography variant="h3" className="song-title" align="center">
          {song.title}
        </Typography>
        <Typography variant="h5" className="song-title" align="center">
          {song.artist}
        </Typography>
        <Grid container className="song-player-content" spacing={10} height={'480px'} display={'flex'}>
          {/* we use grid now as later plan to add additional column additions, change spacing if needed*/}
          <Grid flex={'1'} height={'100%'} alignContent={'center'}>
            <YouTubePlayer videoId={song.youTubeId ?? ''} onReady={handlePlayerReady} />
          </Grid>
          <Grid className='right-grid-parent'>
            <Box className='tabs-grid-parent'>
              <Tabs value={selectedTab} onChange={handleTabChange} aria-label="lyric-tabs" variant="fullWidth">
                <Tab label="Original Lyrics" />
                <Tab label="Romanized Lyrics" />
                <Tab label="Translated Lyrics" />
              </Tabs>
            </Box>
            {/* <LyricDisplay rawLrcLyrics={song.lrcLyrics} currentTime={currentTime} isPlaying={isPlaying}/> */}
            <Box className='lrc-grid-parent'>
              <LyricDisplay 
                rawLrcLyrics={
                  selectedTab === 0 
                    ? song.lrcLyrics ?? 'Not supported'
                    : selectedTab === 1
                      ? song.lrcRomanizedLyrics ?? 'Not supported'
                      : song.lrcTranslatedLyrics ?? 'Not supported'
                } 
                currentTime={currentTime} 
                isPlaying={isPlaying} 
              />
            </Box>
          </Grid>
        </Grid>
      </Container>
    </div>
  );
};

export default SongPlayerPage;
