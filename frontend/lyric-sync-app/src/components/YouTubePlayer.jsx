import React, { useState } from 'react';
import YouTube from 'react-youtube';

const YouTubePlayer = ({ videoId, onReady, onPlay, onPause, onSeek }) => {
  const [player, setPlayer] = useState(null);
  const [isPlaying, setIsPlaying] = useState(false);

  const opts = {
    height: '0', // Hide the video itself (audio only)
    width: '0',
    playerVars: {
      autoplay: 1, // Autoplay the video
      controls: 0, // Hide the video controls
    },
  };

  const handleReady = (event) => {
    const playerInstance = event.target;
    setPlayer(playerInstance);
    onReady(playerInstance);
  };

  return (
    <div>
      <YouTube
        videoId={videoId}
        opts={opts}
        onReady={handleReady}
      />
    </div>
  );
};

export default YouTubePlayer;
