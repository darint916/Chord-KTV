import React, { useEffect, useRef, useState } from 'react';
import YouTubePlayer from '../YouTubePlayer/YouTubePlayer';

interface AudioSnippetPlayerProps {
  videoId: string;
  startTime: number;      // in seconds
  endTime: number;        // in seconds
  play: boolean;          // when true, start snippet; when false, stop/reset
  onEnded: () => void;    // called when snippet naturally ends
}

interface YouTubePlayer {
  seekTo: (_seconds: number, _allowSeekAhead: boolean) => void;
  playVideo: () => void;
  pauseVideo: () => void;
  getCurrentTime: () => number;
  getDuration: () => number;
  setVolume: (_volume: number) => void;
}

/**
 * Renders a hidden YouTubePlayer and controls it to play [startTime … endTime].
 */
const AudioSnippetPlayer: React.FC<AudioSnippetPlayerProps> = ({
  videoId,
  startTime,
  endTime,
  play,
  onEnded,
}) => {
  // const playerRef = useRef<YT.Player | null>(null);
  const playerRef = useRef<YouTubePlayer | null>(null);
  const intervalRef = useRef<number>();
  const [isReady, setIsReady] = useState(false);

  // Track the last videoId to know when to remount the player
  const lastVideoIdRef = useRef<string | null>(null);

  // Reset readiness when videoId changes
  useEffect(() => {
    if (lastVideoIdRef.current !== videoId) {
      setIsReady(false);
      lastVideoIdRef.current = videoId;
    }
  }, [videoId]);

  // Helper to start the snippet safely
  const safeStartSnippet = (player: YouTubePlayer) => {
    try {
      player.seekTo(startTime, true);
      player.playVideo();
    } catch {
      // console.error('[AudioSnippetPlayer] safeStartSnippet failed', err);
    }
  };

  // Play/pause/seek logic for each question
  useEffect(() => {
    const player = playerRef.current;
    if (!player || !isReady) {
      if (play) {
        // TODO: Could show a loading spinner here
        // console.log('[AudioSnippetPlayer] Player not ready yet.');
      }
      return;
    }

    // Stop/reset
    if (!play) {
      try {
        player.pauseVideo();
        player.seekTo(startTime, false);
      } catch {
        // console.warn('[AudioSnippetPlayer] reset error', err);
      }
      if (intervalRef.current) {
        window.clearInterval(intervalRef.current);
      }
      return;
    }

    // Play snippet
    safeStartSnippet(player);

    intervalRef.current = window.setInterval(() => {
      const current = player.getCurrentTime();
      if (current >= endTime) {
        player.pauseVideo();
        player.seekTo(startTime, false);
        onEnded();
        if (intervalRef.current) {
          window.clearInterval(intervalRef.current);
        }
      }
    }, 100);

    // Cleanup on unmount or stop
    return () => {
      if (intervalRef.current) {
        window.clearInterval(intervalRef.current);
      }
    };
  }, [play, startTime, endTime, onEnded, isReady]);

  return (
    <div style={{ width: 0, height: 0, overflow: 'hidden' }}>
      <YouTubePlayer
        key={videoId} // Only remount when videoId changes
        videoId={videoId}
        onReady={player => {
          playerRef.current = player;
          setIsReady(true);
        }}
      />
    </div>
  );
};

export default AudioSnippetPlayer; 