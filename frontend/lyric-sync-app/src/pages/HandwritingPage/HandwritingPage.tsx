import React from 'react';
import HandwritingCanvas from '../../components/HandwritingCanvas/HandwritingCanvas';
import { Container, Box, Typography, Button } from '@mui/material';
import { useSong } from '../../contexts/SongContext';
import './HandwritingPage.scss';
import { useState, useMemo } from 'react';
import { LanguageCode } from '../../api';

const HandwritingPage: React.FC = () => {
  const { song, quizQuestions } = useSong();
  const [currentWordIndex, setCurrentWordIndex] = useState(0);
  const [completedWords, setCompletedWords] = useState<number[]>([]);

  if (!quizQuestions || quizQuestions.length === 0) {
    return <Typography variant="h5">Error: Failed to load handwriting quiz as reading quiz questions were not found</Typography>;
  }

  if (!song || !song.geniusMetaData) {
    return <Typography variant="h5">Error: Song data is undefined or corrupted</Typography>;
  }
  
  const segmenter = new Intl.Segmenter(song.geniusMetaData.language, { granularity: 'word' });

  // Get the longest word from each lyric phrase
  const getLongestWord = (text: string): string => {
    const segments = Array.from(segmenter.segment(text));
    const words = segments
      .filter(segment => segment.isWordLike)
      .map(segment => segment.segment);
    
    if (words.length === 0) return text; // fallback if no word-like segments found
    
    return words.reduce((longest, current) => 
      current.length > longest.length ? current : longest
    );
  };

  const wordsToPractice = quizQuestions
    .map(q => getLongestWord(q.lyricPhrase))
    .filter(word => word.length > 0); // Remove empty strings

  const currentWord = wordsToPractice[currentWordIndex % wordsToPractice.length];
  
  const handleWordCompleted = () => {
    setCompletedWords([...completedWords, currentWordIndex]);
    setCurrentWordIndex((currentWordIndex + 1) % wordsToPractice.length);
  };

  console.log(wordsToPractice);

  return (
    <Container maxWidth="md" className="handwriting-page-container">
      <Typography variant="h4" gutterBottom>
        Handwriting Practice
      </Typography>
      <Typography variant="h6" gutterBottom>
        Current word: {currentWord}
      </Typography>
      
      <Box className="handwriting-canvas-wrapper">
        <HandwritingCanvas 
          expectedText={currentWord}
          selectedLanguage={song.geniusMetaData.language as LanguageCode}
          onComplete={handleWordCompleted}
        />
      </Box>
      
      <Box mt={2}>
        <Typography variant="body1">
          Progress: {completedWords.length} of {wordsToPractice.length} words completed
        </Typography>
        <Button 
          variant="outlined" 
          onClick={() => setCurrentWordIndex((currentWordIndex + 1) % wordsToPractice.length)}
          sx={{ mt: 2 }}
        >
          Skip to Next Word
        </Button>
      </Box>
    </Container>
  );
};

export default HandwritingPage;
