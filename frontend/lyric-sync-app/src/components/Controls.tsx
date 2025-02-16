import React, { useState } from 'react';
import { FontAwesomeIcon } from '@fortawesome/react-fontawesome';
import { faPlay, faPause } from '@fortawesome/free-solid-svg-icons';

interface YouTubePlayer {
  seekTo: (_seconds: number, _allowSeekAhead: boolean) => void;
  playVideo: () => void;
  pauseVideo: () => void;
  setVolume: (_volume: number) => void;
}

interface ControlsProps {
  playerRef: React.RefObject<YouTubePlayer>;
  currentTime: number;
  duration: number;
  onSeek: (_seekTime: number) => void;
  onPlayPause: (_isPlaying: boolean) => void;
  isPlaying: boolean;
}

const Controls: React.FC<ControlsProps> = ({
  playerRef,
  currentTime,
  duration,
  onSeek,
  onPlayPause,
  isPlaying,
}) => {
  const [seekValue, setSeekValue] = useState<number>(currentTime);
  const [volume, setVolume] = useState<number>(100);

  const handleSeekChange = (event: React.ChangeEvent<HTMLInputElement>) => {
    const value = parseFloat(event.target.value);
    setSeekValue(value);
    if (playerRef.current) {
      playerRef.current.seekTo(value, true); // Seek to new time in seconds
    }
    onSeek(value); // Update the current time state
  };

  const handlePlayPause = () => {
    if (isPlaying) {
      playerRef.current?.pauseVideo();
    } else {
      playerRef.current?.playVideo();
    }
    onPlayPause(!isPlaying);
  };

  const handleVolumeChange = (event: React.ChangeEvent<HTMLInputElement>) => {
    const value = parseInt(event.target.value);
    setVolume(value);
    if (playerRef.current) {
      playerRef.current.setVolume(value);
    }
  };

  // Format time from seconds into minutes:seconds format
  const formatTime = (timeInSeconds: number): string => {
    const minutes = Math.floor(timeInSeconds / 60);
    const seconds = Math.floor(timeInSeconds % 60);
    return `${minutes}:${seconds < 10 ? '0' : ''}${seconds}`;
  };

  return (
    <div style={styles.controlsContainer}>
      <div style={styles.controlsWrapper}>
        {/* Play/Pause Icon */}
        <div onClick={handlePlayPause} style={styles.playPauseIcon}>
          <FontAwesomeIcon
            icon={isPlaying ? faPause : faPlay}
            style={styles.icon}
          />
        </div>

        {/* Seek slider */}
        <input
          type="range"
          min="0"
          max={duration || 100}
          value={seekValue}
          onChange={handleSeekChange}
          style={styles.seekBar}
        />
        <span style={styles.timeText}>
          {formatTime(currentTime)} / {formatTime(duration)}
        </span>

        {/* Volume control */}
        <input
          type="range"
          min="0"
          max="100"
          value={volume}
          onChange={handleVolumeChange}
          style={styles.volumeBar}
        />
      </div>
    </div>
  );
};

const styles = {
  controlsContainer: {
    width: '100%',
    display: 'flex',
    justifyContent: 'center',
    padding: '20px 0',
  },
  controlsWrapper: {
    display: 'flex',
    alignItems: 'center',
    padding: '15px',
    backgroundColor: '#000',
    borderRadius: '8px',
    width: '70%',
    justifyContent: 'space-between',
  },
  playPauseIcon: {
    cursor: 'pointer',
    padding: '10px',
  },
  icon: {
    fontSize: '24px', // Set icon size
    color: 'white',
  },
  seekBar: {
    width: '60%',
    margin: '0 15px',
    cursor: 'pointer',
  },
  volumeBar: {
    width: '100px',
    marginLeft: '15px',
    cursor: 'pointer',
  },
  timeText: {
    color: 'white',
    marginLeft: '10px',
    fontSize: '14px',
  },
};

export default Controls;