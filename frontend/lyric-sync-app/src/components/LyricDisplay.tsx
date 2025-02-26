import React from 'react';
import { Box, Typography } from '@mui/material';
import '../styles/components/LyricDisplay.scss';
interface Lyric {
  time: number;
  text: string;
}

interface LyricDisplayProps {
  lyrics: Lyric[];
  currentTime: number;
}

const LyricDisplay: React.FC<LyricDisplayProps> = ({ lyrics, currentTime }) => {
  // Find the lyric that corresponds to the current time
  const currentLyric = lyrics.find((lyric, index) => {
    const nextLyricTime = lyrics[index + 1]?.time || Infinity;
    return currentTime >= lyric.time && currentTime < nextLyricTime;
  });

  return (
    <Box className="lyric-display-container">
      <Typography variant="h4" component="div" className="lyric-text">
        {currentLyric ? currentLyric.text : '♫ ♫'}
      </Typography>
    </Box>
  );
};

export default LyricDisplay;