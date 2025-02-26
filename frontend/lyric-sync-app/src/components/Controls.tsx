import React, { useState } from 'react';
import { FontAwesomeIcon } from '@fortawesome/react-fontawesome';
import { faPlay, faPause } from '@fortawesome/free-solid-svg-icons';
import '../styles/components/Controls.scss';

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
    <div className="controls-container">
      <div className="controls-wrapper">
        <div onClick={handlePlayPause} className="play-pause-icon">
          <FontAwesomeIcon
            icon={isPlaying ? faPause : faPlay}
            className="icon"
          />
        </div>
        <input
          type="range"
          min="0"
          max={duration || 100}
          value={seekValue}
          onChange={handleSeekChange}
          className="seek-bar"
        />
        <span className="time-text">
          {formatTime(currentTime)} / {formatTime(duration)}
        </span>
        <input
          type="range"
          min="0"
          max="100"
          value={volume}
          onChange={handleVolumeChange}
          className="volume-bar"
        />
      </div>
    </div>
  );
};

export default Controls;