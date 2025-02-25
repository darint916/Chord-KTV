import React from 'react';
import { Box, Typography } from '@mui/material';

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
    <Box
      sx={{
        position: 'absolute', 
        bottom: 0,
        top: 0,
        left: 0,
        width: '100vw',
        height: '100vh',
        display: 'flex',
        justifyContent: 'center',
        alignItems: 'center',
        backgroundColor: 'rgba(0, 0, 0, 0.9)', 
        zIndex: -1,
      }}
    >
      <Typography
        variant="h3"
        component="div"
        sx={{
          color: '#fff',
          textAlign: 'center',
          fontSize: { xs: '24px', sm: '36px', lg: '48px' }, // Larger text
          fontWeight: 'bold',
          lineHeight: 1.4,
          textTransform: 'uppercase',
        }}
      >
        {currentLyric ? currentLyric.text : '♫ ♫'}
      </Typography>
    </Box>
  );
};

export default LyricDisplay;
