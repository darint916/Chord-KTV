import React from 'react';
import QuizComponent from '../../components/QuizComponent/QuizComponent';
import { useSong } from '../../contexts/SongContext';
import './QuizPage.scss';

const QuizPage: React.FC = () => {
  const { song } = useSong();

  if (!song || !song.id || song.id.trim() === '') {
    return <div>Error: Song ID not defined.</div>;
  }
  
  return (
    <div className='quiz-page'>
      <QuizComponent songId={song.id} />
    </div>
  );
};

export default QuizPage;
