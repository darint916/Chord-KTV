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
  // Find the index of the current lyric
  const currentIndex = lyrics.findIndex((lyric, index) => {
    const nextLyricTime = lyrics[index + 1]?.time || Infinity;
    return currentTime >= lyric.time && currentTime < nextLyricTime;
  });

  const currentLyric = lyrics[currentIndex] || null;
  const previousLyric = lyrics[currentIndex - 1] || null;
  const nextLyric = lyrics[currentIndex + 1] || null;

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
        flexDirection: 'column',
        justifyContent: 'center',
        alignItems: 'center',
        backgroundColor: 'rgba(0, 0, 0, 0.9)',
        zIndex: -1,
      }}
    >
      {/* Previous lyric (faded) */}
      {previousLyric && previousLyric.text.trim() && (
        <Typography
          variant="h5"
          component="div"
          sx={{
            color: 'rgba(255, 255, 255, 0.5)',
            textAlign: 'center',
            fontSize: { xs: '10px', sm: '20px', lg: '30px' },
            fontWeight: 'bold',
            lineHeight: 1.4,
            textTransform: 'uppercase',
            mb: 2, // Margin bottom to separate from current lyric
          }}
        >
          {previousLyric.text}
        </Typography>
      )}

      {/* Current lyric */}
      <Typography
        variant="h3"
        component="div"
        sx={{
          color: '#fff',
          textAlign: 'center',
          fontSize: { xs: '12px', sm: '24px', lg: '36px' },
          fontWeight: 'bold',
          lineHeight: 1.4,
          textTransform: 'uppercase',
        }}
      >
        {currentLyric && currentLyric.text?.trim() ? currentLyric.text : '♫ ♫'}
      </Typography>

      {/* Next lyric (faded) */}
      {nextLyric && nextLyric.text.trim() && (
        <Typography
          variant="h5"
          component="div"
          sx={{
            color: 'rgba(255, 255, 255, 0.5)',
            textAlign: 'center',
            fontSize: { xs: '10px', sm: '20px', lg: '30px' },
            fontWeight: 'bold',
            lineHeight: 1.4,
            textTransform: 'uppercase',
            mt: 2, // Margin top to separate from current lyric
          }}
        >
          {nextLyric.text}
        </Typography>
      )}
    </Box>
  );
};

export default LyricDisplay;
