import React, { useEffect, useRef, useCallback } from 'react';
import { Box, Typography } from '@mui/material';
import { Lrc, LrcLine } from 'react-lrc';
import './LyricDisplay.scss';
// import { height } from '@fortawesome/free-solid-svg-icons/fa0';
interface Lyric {
  time: number;
  text: string;
}

interface LyricDisplayProps {
  lyrics: Lyric[];
  rawLrcLyrics: string;
  currentTime: number;
}

const LyricDisplay: React.FC<LyricDisplayProps> = ({ lyrics, rawLrcLyrics, currentTime }) => { // we use https://github.com/mebtte/react-lrc, uses ms for time though

  const lineRenderer = useCallback(({ active, line }: { active: boolean; line: LrcLine }) => (
    <div className={`lyric-text ${active ? 'active' : ''}`}>
      {line.content}
    </div>
  ), []
  );
  return ( //autoscroll turned on alr, consider reading docs if we want to reformat line styling and distances
    <Box className="lyric-display-container lrc">
      <Lrc
        lrc={rawLrcLyrics}
        currentMillisecond={currentTime * 1000}
        lineRenderer={lineRenderer}
        verticalSpace
      />
    </Box>
  );
};

export default LyricDisplay;
