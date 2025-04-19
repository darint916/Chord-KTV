import React, { useState, useRef, useMemo, useEffect } from 'react';
import { Container, Typography, Box, Button } from '@mui/material';
import YouTubePlayer from '../../components/YouTubePlayer/YouTubePlayer';
import LyricDisplay from '../../components/LyricDisplay/LyricDisplay';
import './SongPlayerPage.scss';
import Grid from '@mui/material/Grid2';
import Tabs from '@mui/material/Tabs';
import Tab from '@mui/material/Tab';
import { useSong } from '../../contexts/SongContext';
import { useNavigate } from 'react-router-dom';
import YouTubePlaylistViewer from '../../components/YouTubePlaylistViewer/YouTubePlaylistViewer';
import { useLocation } from 'react-router-dom';

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
  const { song, setQuizQuestions } = useSong();
  const [selectedTab, setSelectedTab] = useState(0);
  const [showQuizButton, setShowQuizButton] = useState(false);
  const navigate = useNavigate();
  const location = useLocation();
  const { playlistUrl } = location.state || {};
  let animationFrameId: number;

  if (!song) {
    return <Typography variant="h5">Error: No song selected</Typography>;
  }

  if (!song.lrcLyrics || !song.lrcLyrics.trim()) {
    return <Typography variant="h5">Error: No time-synced lyrics found for song</Typography>;
  }

  if (!song.youTubeId || !song.youTubeId.trim()) {
    return <Typography variant="h5">Error: No YouTube video found for song</Typography>;
  }

  const lrcTimestamps = useMemo(() => {
    const timestamps: number[] = [];
    if (!song.lrcLyrics) {
      return timestamps;
    };
    const timeTagRegex = /\[(\d+):(\d+)\.(\d+)\]/g;
    const timeMatches = [...song.lrcLyrics.matchAll(timeTagRegex)];
    timeMatches.forEach(match => {
      const minutes = parseInt(match[1]);
      const seconds = parseInt(match[2]);
      const centisecond = parseInt(match[3]);
      timestamps.push(minutes * 60 + seconds + (centisecond / 100));
    });
    return timestamps.sort((a, b) => a - b);
  }, [song.lrcLyrics]);


  const prevTimeRange = useRef({ start: Infinity, end: 0 });
  const checkIfTimeLineChanged = (currentTime: number, timestamps: number[]) => {
    if (timestamps.length === 0 || (currentTime >= prevTimeRange.current.start && currentTime < prevTimeRange.current.end)) {
      return false;
    }
    for (let i = 0; i < timestamps.length; i++) {
      const currentTimestamp = timestamps[i];
      const nextTimestamp = (i < timestamps.length - 1) ? timestamps[i + 1] : Infinity;
      if (currentTime >= currentTimestamp && currentTime < nextTimestamp) {
        prevTimeRange.current = { start: currentTimestamp, end: nextTimestamp };
        break;
      }
    }
    return true;
  };

  const allowedQuizLanguages = new Set(['AR', 'BG', 'BN', 'EL', 'FA', 'GU', 'HE', 'HI', 'JA', 'KO', 'RU', 'SR', 'TA', 'TE', 'TH', 'UK', 'ZH']);
  const isLanguageAllowedForQuiz = song.geniusMetaData?.language && allowedQuizLanguages.has(song.geniusMetaData.language);

  const updatePlayerTime = (playerInstance: YouTubePlayer) => {
    playerRef.current = playerInstance;
    playerInstance.playVideo(); // Autoplay

    const updatePlayerTime = () => {
      if (playerRef.current) {
        const current = playerRef.current.getCurrentTime();
        if (checkIfTimeLineChanged(current, lrcTimestamps)) {
          setCurrentTime(current);
        }
        // Check if the song is 90% complete
        if (current / playerRef.current.getDuration() >= 0.9 && isLanguageAllowedForQuiz) {
          setShowQuizButton(true); // Show the quiz button when 90% complete
        }
      }
      animationFrameId = requestAnimationFrame(updatePlayerTime); //req next frame
    };

    updatePlayerTime();
  };

  useEffect(() => { //cleanup on rerender
    return () => {
      if (animationFrameId) {
        cancelAnimationFrame(animationFrameId); // Cancel the animation frame when the component unmounts  (cleanup function)
      };
    };
  }, []);

  const handleTabChange = (_event: React.SyntheticEvent, newValue: number) => {
    setSelectedTab(newValue);
  };

  const handleQuizRedirect = () => {
    setQuizQuestions([]);   // Clear old song quiz questions
    navigate('/quiz');
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
        {showQuizButton && (
          <Box mt={3} display="flex" justifyContent="center">
            <Button
              variant="contained"
              onClick={handleQuizRedirect}
              className="quiz-button"
            >
              Go to Quiz
            </Button>
          </Box>
        )}
        <Grid container className="song-player-content" spacing={10} height={'480px'} display={'flex'}>
          {/* we use grid now as later plan to add additional column additions, change spacing if needed*/}
          <Grid flex={'1'} alignContent={'center'} className='grid-parent'>
            <YouTubePlayer videoId={song.youTubeId ?? ''} onReady={updatePlayerTime} />
          </Grid>
          <Grid className='grid-parent'>
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
        {playlistUrl && <YouTubePlaylistViewer playlistUrl={playlistUrl} />}
      </Container>
    </div>
  );
};

export default SongPlayerPage;
