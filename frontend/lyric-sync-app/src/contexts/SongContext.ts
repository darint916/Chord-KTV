import { createContext, useContext } from 'react';
import { FullSongResponseDto } from '../api';
import { QuizQuestionDto } from '../api';
import { QueueItem } from './QueueTypes';

export interface PlaylistInfo {
  playlistId: string;
  title: string;
  isFavorite: boolean;
}

// Define the context type
export interface SongContextType {
  song: FullSongResponseDto | null;
  quizQuestions?: QuizQuestionDto[] | null;
  setSong: (_song: FullSongResponseDto | null) => void;
  setQuizQuestions: (_quiz: QuizQuestionDto[]) => void;
  queue: QueueItem[];
  setQueue: (_queue: QueueItem[] | ((_prevQueue: QueueItem[]) => QueueItem[])) => void;
  currentPlayingId: string | null;
  setCurrentPlayingId: React.Dispatch<React.SetStateAction<string | null>>;
  lyricsOffset: number;
  setLyricsOffset: React.Dispatch<React.SetStateAction<number>>;
  playlists: PlaylistInfo[];
  setPlaylists: React.Dispatch<React.SetStateAction<PlaylistInfo[]>>;
  selectedPlaylistIndex: number;
  setSelectedPlaylistIndex: React.Dispatch<React.SetStateAction<number>>;
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