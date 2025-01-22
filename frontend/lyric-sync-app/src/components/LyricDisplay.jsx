import React from 'react';

const LyricDisplay = ({ lyrics, currentTime }) => {
  // Find the lyric that corresponds to the current time
  const currentLyric = lyrics.find((lyric, index) => {
    // Ensure that we find the correct lyric which hasn't already passed
    const nextLyricTime = lyrics[index + 1]?.time || Infinity;
    return currentTime >= lyric.time && currentTime < nextLyricTime;
  });

  return (
    <div style={{ padding: '20px', fontSize: '24px', textAlign: 'center', color: '#fff' }}>
      {currentLyric ? currentLyric.text : '♫ ♫'}
    </div>
  );
};

export default LyricDisplay;
