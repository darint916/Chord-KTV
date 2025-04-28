import React from 'react';
import QuizComponent from '../../components/QuizComponent/QuizComponent';
import { useSong } from '../../contexts/SongContext';
import { useSearchParams } from 'react-router-dom';
import './QuizPage.scss';

const QuizPage: React.FC = () => {
  const { song } = useSong();
  const [searchParams] = useSearchParams();
  const urlSongId = searchParams.get('id');
  const urlOffset = searchParams.get('offset');
  const lyricsOffset = urlOffset ? Number(urlOffset) : 0;

  // Prefer the song ID from the URL query parameter if available
  const effectiveSongId = urlSongId || (song && song.id && song.id.trim() !== '' ? song.id : null);

  if (!effectiveSongId) {
    return <div>Error: Song ID not defined.</div>;
  }
  
  return (
    <div className='quiz-page'>
      <QuizComponent songId={effectiveSongId} lyricsOffset={lyricsOffset} />
    </div>
  );
};

export default QuizPage;
