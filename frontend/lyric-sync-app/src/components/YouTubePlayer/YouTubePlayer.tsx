import React, { useState } from 'react';
import YouTube, { YouTubePlayer as YouTubePlayerInstance } from 'react-youtube';

interface YouTubePlayerProps {
  videoId: string;
  onReady: (_playerInstance: YouTubePlayerInstance) => void;
  onEnd?: () => void;
}

const YouTubePlayer: React.FC<YouTubePlayerProps> = ({ videoId, onReady, onEnd }) => {
  const setPlayer = useState<YouTubePlayerInstance | null>(null)[1]; // Extract only the setter

  const opts = {
    width: '100%',
    playerVars: { 'autoplay': 1 }
  };

  const handleEnd = () => {
    if (onEnd) { onEnd(); }
  };

  const handleReady = (event: { target: YouTubePlayerInstance }) => {
    const playerInstance = event.target;
    setPlayer(playerInstance);
    onReady(playerInstance);
  };

  return (
    <div>
      <YouTube videoId={videoId} className="youtube-player" opts={opts} onReady={handleReady} onEnd={handleEnd} />
    </div>
  );
};

export default YouTubePlayer;
