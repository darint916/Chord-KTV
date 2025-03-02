/* eslint-disable react/prop-types */
import { useState } from 'react';
import { FullSongResponseDto } from '../api';
import { SongContext } from './SongContext';

// Provider component
export const SongProvider: React.FC<{ children: React.ReactNode }> = ({ children }) => {
  const [song, setSong] = useState<FullSongResponseDto | null>(null);

  return (
    <SongContext.Provider value={{ song, setSong }}>
      {children}
    </SongContext.Provider>
  );
};
