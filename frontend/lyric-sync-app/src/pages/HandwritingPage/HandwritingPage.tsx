import React, { useRef, useState } from 'react';
import HandwritingCanvas from '../../components/HandwritingCanvas/HandwritingCanvas';
import { Container, Box, Typography, Button } from '@mui/material';
import { useSong } from '../../contexts/SongContext';
import './HandwritingPage.scss';
import { LanguageCode } from '../../api';

const HandwritingPage: React.FC = () => {
  const { song, quizQuestions } = useSong();
  const [currentWordIndex, setCurrentWordIndex] = useState(0);
  const [completedWords, setCompletedWords] = useState<number[]>([]);
  const [currentWordCompleted, setCurrentWordCompleted] = useState(false);
  const handwritingCanvasRef = useRef<{ clearCanvas: () => void }>(null);

  if (!quizQuestions || quizQuestions.length === 0) {
    return <Typography variant="h5">Error: Failed to load handwriting quiz as reading quiz questions were not found</Typography>;
  }

  if (!song || !song.geniusMetaData) {
    return <Typography variant="h5">Error: Song data is undefined or corrupted</Typography>;
  }
  
  const segmenter = new Intl.Segmenter(song.geniusMetaData.language, { granularity: 'word' });

  const getLongestWord = (text: string): string => {
    const segments = Array.from(segmenter.segment(text))
        .filter(segment => segment.isWordLike)
        .map(segment => segment.segment);
    
    if (segments.length === 0) return '';
    
    return segments.reduce((longest, current) => 
        current.length > longest.length ? current : longest
    );
  };
  
  const wordsToPractice = quizQuestions
    .map(q => getLongestWord(q.lyricPhrase ?? ''))
    .filter(word => word.length > 0);
  
  const currentWord = wordsToPractice[currentWordIndex % wordsToPractice.length];
  
  const handleWordCompletionAttempt = (isSuccess: boolean) => {
    if (isSuccess) {
      setCompletedWords([...completedWords, currentWordIndex]);
      setCurrentWordCompleted(true);
    }
  };

  const moveToNextWord = () => {
    if (handwritingCanvasRef.current) {
      handwritingCanvasRef.current.clearCanvas();
    }
    setCurrentWordIndex(currentWordIndex + 1);
    setCurrentWordCompleted(false);
  };

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
          ref={handwritingCanvasRef}
          expectedText={currentWord}
          selectedLanguage={song.geniusMetaData.language as LanguageCode}
          onComplete={handleWordCompletionAttempt}
        />
      </Box>
      
      <Box mt={2}>
        <Typography variant="body1">
          Progress: {completedWords.length} of {wordsToPractice.length} words completed
        </Typography>
        
        {currentWordCompleted && (
          <Button 
            variant="contained" 
            color="success"
            onClick={moveToNextWord}
            sx={{ mt: 2, mr: 2 }}
          >
            Next Word
          </Button>
        )}

        <Button 
          variant="outlined"
          color="error"
          onClick={moveToNextWord}
          sx={{ mt: 2 }}
        >
          Skip
        </Button>
      </Box>
    </Container>
  );
};

export default HandwritingPage;
