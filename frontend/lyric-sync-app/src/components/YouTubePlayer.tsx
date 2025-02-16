import React, { useState } from 'react';
import YouTube from 'react-youtube';

interface YouTubePlayerProps {
  videoId: string;
  onReady: (playerInstance: any) => void;
}

const YouTubePlayer: React.FC<YouTubePlayerProps> = ({ videoId, onReady }) => {
  const [_, setPlayer] = useState<any>(null);

  const opts = {
    height: '0', // Hide the video itself (audio only)
    width: '0',
    playerVars: {
    //   autoplay: 1, // Autoplay the video
      controls: 0, // Hide the video controls
    },
  };

  const handleReady = (event: any) => {
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