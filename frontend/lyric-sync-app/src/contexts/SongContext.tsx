/* eslint-disable react/prop-types */
import { createContext, useContext, useState } from 'react';
import { FullSongResponseDto } from '../api';

// Define the context type
interface SongContextType {
  song: FullSongResponseDto | null;
  setSong: (_song: FullSongResponseDto) => void;
}

// Create context
export const SongContext = createContext<SongContextType | undefined>(undefined);

// Provider component
export const SongProvider: React.FC<{ children: React.ReactNode }> = ({ children }) => {
  const [song, setSong] = useState<FullSongResponseDto | null>(null);

  return (
    <SongContext.Provider value={{ song, setSong }}>
      {children}
    </SongContext.Provider>
  );
};
