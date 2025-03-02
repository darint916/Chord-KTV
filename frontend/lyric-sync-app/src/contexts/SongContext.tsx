import { createContext, useContext, useState } from 'react';
import { FullSongResponseDto } from '../api';

// Define the context type
interface SongContextType {
  song: FullSongResponseDto | null;
  setSong: (song: FullSongResponseDto) => void;
}

// Create context
const SongContext = createContext<SongContextType | undefined>(undefined);

// Provider component
export const SongProvider: React.FC<{ children: React.ReactNode }> = ({ children }) => {
  const [song, setSong] = useState<FullSongResponseDto | null>(null);

  return (
    <SongContext.Provider value={{ song, setSong }}>
      {children}
    </SongContext.Provider>
  );
};

// Hook to use song context
export const useSong = () => {
  const context = useContext(SongContext);
  if (!context) {
    throw new Error('useSong must be used within a SongProvider');
  }
  return context;
};
