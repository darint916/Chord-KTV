import React, { useState } from 'react';

const Controls = ({ playerRef, currentTime, duration, onSeek, onPlayPause, isPlaying }) => {
  const [seekValue, setSeekValue] = useState(currentTime);
  const [volume, setVolume] = useState(100);

  const handleSeekChange = (event) => {
    const value = event.target.value;
    setSeekValue(value);
    if (playerRef.current) {
      playerRef.current.seekTo(value, true); // Seek to new time in seconds
    }
    onSeek(value); // Update the current time state
  };

  const handlePlayPause = () => {
    if (isPlaying) {
      playerRef.current.pauseVideo();
    } else {
      playerRef.current.playVideo();
    }
    onPlayPause(!isPlaying);
  };

  const handleVolumeChange = (event) => {
    const value = event.target.value;
    setVolume(value);
    if (playerRef.current) {
      playerRef.current.setVolume(value);
    }
  };

    // Format time from seconds into minutes:seconds format
  const formatTime = (timeInSeconds) => {
    const minutes = Math.floor(timeInSeconds / 60);
    const seconds = Math.floor(timeInSeconds % 60);
    return `${minutes}:${seconds < 10 ? '0' : ''}${seconds}`;
  };

    return (
    <div style={styles.controlsWrapper}>
      {/* Play/Pause button */}
      <button style={styles.playPauseButton} onClick={handlePlayPause}>
        {isPlaying ? 'Pause' : 'Play'}
      </button>

      {/* Seek slider */}
      <input
        type="range"
        min="0"
        max={duration || 100}
        value={seekValue}
        onChange={handleSeekChange}
        style={styles.seekBar}
      />
      <span>
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
  );
};

const styles = {
  controlsWrapper: {
    display: 'flex',
    borderRadius: '15px', 
    alignItems: 'center',
    padding: '10px',
    backgroundColor: '#000',
    color: '#fff',
  },
  playPauseButton: {
    padding: '10px',
    marginRight: '15px',
    fontSize: '16px',
    color: 'white',
    backgroundColor: '#000',
    border: 'none',
    cursor: 'pointer',
  },
  seekBar: {
    marginRight: '15px',
    cursor: 'pointer',
  },
  volumeBar: {
    marginLeft: '15px',
    width: '100px',
    cursor: 'pointer',
  },
};

export default Controls;