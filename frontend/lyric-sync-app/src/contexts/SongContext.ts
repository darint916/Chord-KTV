import { createContext, useContext } from 'react';
import { FullSongResponseDto } from '../api';

// Define the context type
interface SongContextType {
  song: FullSongResponseDto | null;
  setSong: (_song: FullSongResponseDto) => void;
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