/* eslint-disable react/prop-types */
import { useState, useEffect } from 'react';
import { FullSongResponseDto } from '../api';
import { SongContext } from './SongContext';

// Provider component
export const SongProvider: React.FC<{ children: React.ReactNode }> = ({ children }) => {
  const [song, setSong] = useState<FullSongResponseDto | null>(() => {
    // Try to get the song from localStorage on initial load
    const savedSong = localStorage.getItem('song');
    return savedSong ? JSON.parse(savedSong) : null;
  });

  useEffect(() => {
    if (song) {
      localStorage.setItem('song', JSON.stringify(song));
    } else {
      localStorage.removeItem('song');
    }
  }, [song]);

  return (
    <SongContext.Provider value={{ song, setSong }}>
      {children}
    </SongContext.Provider>
  );
};
