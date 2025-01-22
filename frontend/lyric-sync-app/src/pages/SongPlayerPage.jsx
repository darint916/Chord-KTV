import React, { useState, useEffect, useRef } from 'react';
import YouTubePlayer from '../components/YouTubePlayer';
import LyricDisplay from '../components/LyricDisplay';
import Controls from '../components/Controls';
import axios from 'axios';

const SongPlayerPage = () => {
  const [lyrics, setLyrics] = useState([]);
  const [currentTime, setCurrentTime] = useState(0);
  const [isPlaying, setIsPlaying] = useState(false);
  const [duration, setDuration] = useState(100);
  const videoId = 'ChQaa0eqZak'; 
  const playerRef = useRef(null);

  useEffect(() => {
    // Fetch LRC file and parse
    const fetchLyrics = async () => {
      try {
        const response = await axios.get(
          'https://lrclib.net/api/get', {
          params: {
            track_name: 'midnight cruisin',
            artist_name: 'Kingo Hamada'
          }
        });
        const parsedLyrics = parseLRC(response.data.syncedLyrics);
        setLyrics(parsedLyrics);
      } catch (err) {
        console.error('Error fetching lyrics:', err);
      }
    };
    fetchLyrics();
  }, []);

  const parseLRC = (lrc) => {
    return lrc.split('\n').map((line) => {
      const match = line.match(/\[(\d+):(\d+\.\d+)](.+)/);
      if (!match) return null;

      const time = parseInt(match[1]) * 60 + parseFloat(match[2]);
      return { time, text: match[3] };
    }).filter(Boolean);
  };

   const handlePlayerReady = (playerInstance) => {
    playerRef.current = playerInstance;

    // Get duration when player is ready
    playerInstance.getDuration && setDuration(playerInstance.getDuration());

    const intervalId = setInterval(() => {
      if (playerRef.current) {
        setCurrentTime(playerRef.current.getCurrentTime());
      }
    }, 500); // Sync every 500ms

    return () => clearInterval(intervalId);
  };

  const handlePlayPause = (playState) => {
    setIsPlaying(playState);
  };

  const handleSeek = (seekTime) => {
    setCurrentTime(seekTime);
  };

    return (
        <div 
        style={{ 
            fontFamily: 'Arial, sans-serif', 
            backgroundColor: '#6a0dad',  // Add your desired background color
            color: 'white',              // Change text color to white for contrast
            height: '100vh',             // Make the page occupy full screen height
            display: 'flex', 
            flexDirection: 'column',     // Keep all components stacked vertically
            justifyContent: 'center',    // Center the content vertically
            alignItems: 'center',        // Center the content horizontally
            padding: '20px',             // Add some space around the content
        }}
        >
        <h1 style={{ fontSize: '36px', color: '#fff', marginBottom: '20px' }}>
            Midnight Cruisin' by Kingo Hamada
        </h1>
        <YouTubePlayer
            videoId={videoId}
            onReady={handlePlayerReady}
        />
        <LyricDisplay lyrics={lyrics} currentTime={currentTime} />
        <Controls
            playerRef={playerRef}
            currentTime={currentTime}
            duration={duration}
            onSeek={handleSeek}
            onPlayPause={handlePlayPause}
            isPlaying={isPlaying}
        />
        </div>
    );
};

export default SongPlayerPage;