import { useState, useEffect } from 'react';
import { FullSongResponseDto, QuizQuestionDto } from '../api';
import { SongContext } from './SongContext';
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

  return (
    <SongContext.Provider value={{ song, setSong, quizQuestions, setQuizQuestions, queue, setQueue, currentPlayingId, setCurrentPlayingId }}>
      {children}
    </SongContext.Provider>
  );
};
