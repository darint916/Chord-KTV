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
        display: 'flex',
        justifyContent: 'center',
        alignItems: 'center',
        backgroundColor: 'rgba(0, 0, 0, 0.7)',
        borderRadius: 2,
        padding: 2,
        maxWidth: '80%',
        marginTop: 3,
        height: 100,  // Fixed height
      }}
    >
      <Typography
        variant="h4"
        component="div"
        sx={{
          color: '#fff',
          textAlign: 'center',
          fontSize: { xs: '18px', sm: '24px', lg: '32px' },
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

