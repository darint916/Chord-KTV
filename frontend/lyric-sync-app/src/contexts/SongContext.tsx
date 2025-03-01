import { createContext, useContext, useState } from 'react';

interface SongContextProps {
  songName: string;
  artistName: string;
  setSong: (song: string, artist: string) => void;
}

const SongContext = createContext<SongContextProps | undefined>(undefined);

export const SongProvider: React.FC<{ children: React.ReactNode }> = ({ children }) => {
  const [songName, setSongName] = useState('');
  const [artistName, setArtistName] = useState('');

  const setSong = (song: string, artist: string) => {
    setSongName(song);
    setArtistName(artist);
  };

  return (
    <SongContext.Provider value={{ songName, artistName, setSong }}>
      {children}
    </SongContext.Provider>
  );
};

export const useSong = () => {
  const context = useContext(SongContext);
  if (!context) {
    throw new Error('useSong must be used within a SongProvider');
  }
  return context;
};
