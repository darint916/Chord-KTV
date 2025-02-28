import React, { useEffect, useRef, useCallback } from 'react';
import { Lrc, LrcLine, useRecoverAutoScrollImmediately } from 'react-lrc';
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
  isPlaying: boolean;
}

const LyricDisplay: React.FC<LyricDisplayProps> = ({ rawLrcLyrics, currentTime, isPlaying }) => { // we use https://github.com/mebtte/react-lrc, uses ms for time though

  const { signal, recoverAutoScrollImmediately} = useRecoverAutoScrollImmediately();
  const wasPlayingRef = useRef(isPlaying);

  useEffect(() => {
    if (isPlaying && !wasPlayingRef.current) {
      recoverAutoScrollImmediately();
    }
    wasPlayingRef.current = isPlaying;
  }, [isPlaying, currentTime]);

  const lineRenderer = useCallback(({ active, line }: { active: boolean; line: LrcLine }) => (
    <div className={`lyric-text ${active ? 'active' : ''}`}>
      {line.content}
    </div>
  ), []
  );

  return ( //autoscroll turned on alr, consider reading docs if we want to reformat line styling and distances
    <div className="lyric-display-container">
      <Lrc
        lrc={rawLrcLyrics}
        currentMillisecond={currentTime * 1000}
        lineRenderer={lineRenderer}
        verticalSpace
        recoverAutoScrollSingal={signal}
        recoverAutoScrollInterval={3000} //3 second for recovery auto
        className="lrc"
      />
    </div>
  );
};

export default LyricDisplay;
