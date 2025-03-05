import React, { useState, useEffect, useRef } from 'react';
import { Container, Typography, Box } from '@mui/material';
import axios from 'axios';
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
  const [videoId, setVideoId] = useState<string | null>(null);
  const { song } = useSong();
  const [selectedTab, setSelectedTab] = useState(0);

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
            q: `${song.title} ${song.artist}`,
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

    setInterval(() => {
      if (playerRef.current) { setCurrentTime(playerRef.current.getCurrentTime()); }
    }, 250);
  };

  const handleTabChange = (event: React.SyntheticEvent, newValue: number) => {
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
        <Grid container className="song-player-content" spacing={5} height={'480px'} display={'flex'}>
          {/* we use grid now as later plan to add additional column additions, change spacing if needed*/}
          <Grid flex={'1'} height={'100%'} alignContent={'center'}>
            <YouTubePlayer videoId={videoId ?? ''} onReady={handlePlayerReady} />
          </Grid>
          <Grid className='right-grid-parent'>
            <Box className='tabs-grid-parent'>
              <Tabs value={selectedTab} onChange={handleTabChange} aria-label="lyric-tabs">
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
                    ? song.lrcLyrics ?? "Not supported"
                    : selectedTab === 1
                    ? song.lrcRomanizedLyrics ?? "Not supported"
                    : song.lrcTranslatedLyrics ?? "Not supported"
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
