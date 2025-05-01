import { useState, useEffect } from 'react';
import { FullSongResponseDto, QuizQuestionDto } from '../api';
import { SongContext, PlaylistInfo } from './SongContext';
import { QueueItem } from './QueueTypes';

// Provider component
export const SongProvider: React.FC<{ children: React.ReactNode }> = ({ children }) => {
  const [song, setSong] = useState<FullSongResponseDto | null>(() => {
    // Try to get the song from localStorage on initial load
    const savedSong = localStorage.getItem('song');
    return savedSong ? JSON.parse(savedSong) : null;
  });

  const [queue, setQueue] = useState<QueueItem[]>(() => {
    const savedQueue = typeof window !== 'undefined' ? localStorage.getItem('songQueue') : null;
    return savedQueue ? JSON.parse(savedQueue) : [];
  });

  const [currentPlayingId, setCurrentPlayingId] = useState<string | null>(() => {
    const savedCurrentId = typeof window !== 'undefined' ? localStorage.getItem('currentPlayingId') : null;
    return savedCurrentId ? JSON.parse(savedCurrentId) : null;
  });

  const [lyricsOffset, setLyricsOffset] = useState<number>(0);

  useEffect(() => {
    if (typeof window !== 'undefined') {
      localStorage.setItem('songQueue', JSON.stringify(queue));
      localStorage.setItem('currentPlayingId', JSON.stringify(currentPlayingId));
    }
  }, [queue, currentPlayingId]);

  const [quizQuestions, setQuizQuestions] = useState<QuizQuestionDto[] | undefined>(() => {
    // Try to get quizQuestions from localStorage on initial load
    const savedQuizQuestions = localStorage.getItem('quizQuestions');
    return savedQuizQuestions ? JSON.parse(savedQuizQuestions) : undefined;
  });

  useEffect(() => {
    if (song) {
      localStorage.setItem('song', JSON.stringify(song));
    } else {
      localStorage.removeItem('song');
    }
  }, [song]);

  useEffect(() => {
    if (quizQuestions) {
      localStorage.setItem('quizQuestions', JSON.stringify(quizQuestions));
    } else {
      localStorage.removeItem('quizQuestions');
    }
  }, [quizQuestions]);

  const [playlists, setPlaylists] = useState<PlaylistInfo[]>(() => {
    try {
      const stored = localStorage.getItem('playlists');
      return stored ? JSON.parse(stored) : [];
    } catch {
      return [];
    }
  });

  const [selectedPlaylistIndex, setSelectedPlaylistIndex] = useState<number>(() => {
    try {
      const storedIndex = localStorage.getItem('selectedPlaylistIndex');
      return storedIndex ? Number(storedIndex) : 0;
    } catch {
      return 0;
    }
  });

  useEffect(() => {
    localStorage.setItem('selectedPlaylistIndex', selectedPlaylistIndex.toString());
  }, [selectedPlaylistIndex]);


  useEffect(() => {
    localStorage.setItem('playlists', JSON.stringify(playlists));
  }, [playlists]);


  return (
    <SongContext.Provider value={{
      song, setSong, queue, setQueue, currentPlayingId, setCurrentPlayingId, quizQuestions, setQuizQuestions, lyricsOffset, setLyricsOffset,
      playlists, setPlaylists, selectedPlaylistIndex, setSelectedPlaylistIndex
    }}>
      {children}
    </SongContext.Provider>
  );
};
