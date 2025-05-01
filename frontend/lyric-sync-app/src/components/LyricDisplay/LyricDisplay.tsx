import React, { useEffect, useRef, useCallback } from 'react';
import { Lrc, LrcLine, useRecoverAutoScrollImmediately } from 'react-lrc';
import './LyricDisplay.scss';
interface LyricDisplayProps {
  rawLrcLyrics: string;
  currentTime: number;
  isPlaying: boolean;
}

const LyricDisplay: React.FC<LyricDisplayProps> = ({ rawLrcLyrics, currentTime, isPlaying }) => { // we use https://github.com/mebtte/react-lrc, uses ms for time though

  const { signal, recoverAutoScrollImmediately } = useRecoverAutoScrollImmediately();
  const wasPlayingRef = useRef(isPlaying);
  useEffect(() => {
    if (isPlaying && !wasPlayingRef.current) {
      recoverAutoScrollImmediately();
    }
    wasPlayingRef.current = isPlaying;
  }, [isPlaying, currentTime]);


  const COMBINED_DELIMITER = '<<<SEP>>>'; // make sure same delimiter

  const lineRenderer = useCallback(({ active, line }: { active: boolean; line: LrcLine }) => {
    const parts = line.content.split(COMBINED_DELIMITER);

    return (
      <div className={`lyric-text ${active ? 'active-lrc' : ''}`}>
        {parts.map((part, index) => (
          <div key={index} className={`lyric-part lyric-part-${index}`}>
            {part.trim() ? part : '♫ ♫'}
          </div>
        ))}
      </div>
    );
  }, []);

  return ( //autoscroll turned on alr, consider reading docs if we want to reformat line styling and distances
    <div className="lyric-display-container">
      <Lrc
        lrc={rawLrcLyrics}
        currentMillisecond={currentTime * 1000}
        lineRenderer={lineRenderer}
        recoverAutoScrollSingal={signal}
        recoverAutoScrollInterval={3000} //3 second for recovery auto
        className="lrc"
      />
    </div >
  );
};

export default LyricDisplay;
