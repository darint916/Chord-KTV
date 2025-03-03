import React, { useState } from 'react';
import YouTube, { YouTubePlayer as YouTubePlayerInstance } from 'react-youtube';

interface YouTubePlayerProps {
  videoId: string;
  onReady: (_playerInstance: YouTubePlayerInstance) => void;
}

const YouTubePlayer: React.FC<YouTubePlayerProps> = ({ videoId, onReady }) => {
  const setPlayer = useState<YouTubePlayerInstance | null>(null)[1]; // Extract only the setter

  const opts = {
    height: '0', // Hide the video itself (audio only)
    width: '0',
    playerVars: {
      // controls: 0, // Hide the video controls
    },
  };

  const handleReady = (event: { target: YouTubePlayerInstance }) => {
    const playerInstance = event.target;
    setPlayer(playerInstance);
    onReady(playerInstance);
  };

  return (
    <div>
      <YouTube videoId={videoId} opts={opts} onReady={handleReady} />
    </div>
  );
};

export default YouTubePlayer;