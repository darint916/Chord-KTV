import { createContext, useContext } from 'react';
import { FullSongResponseDto } from '../api';
import { QuizQuestionDto } from '../api';
import { QueueItem } from './QueueTypes';

// Define the context type
interface SongContextType {
  song: FullSongResponseDto | null;
  quizQuestions?: QuizQuestionDto[] | null;
  setSong: (_song: FullSongResponseDto) => void;
  setQuizQuestions: (_quiz: QuizQuestionDto[]) => void;
  queue: QueueItem[];
  setQueue: (_queue: QueueItem[] | ((_prevQueue: QueueItem[]) => QueueItem[])) => void;
  currentPlayingId: string | null;
  // setCurrentPlayingId: (_id: string | null) => void;
  setCurrentPlayingId: React.Dispatch<React.SetStateAction<string | null>>;
}

// Create context
export const SongContext = createContext<SongContextType | undefined>(undefined);

export const useSong = () => {
  const context = useContext(SongContext);
  if (!context) {
    throw new Error('useSong must be used within a SongProvider');
  }
  return context;
};