import React from 'react';
import HandwritingCanvas from '../../components/HandwritingCanvas/HandwritingCanvas';
import { Container, Box, Typography } from '@mui/material';
import { useSong } from '../../contexts/SongContext';
import './HandwritingPage.scss';

const HandwritingPage: React.FC = () => {
  const { quizQuestions } = useSong();

  if (!quizQuestions || quizQuestions.length === 0) {
    return <Typography variant="h5">Error: Failed to load handwriting quiz as reading quiz questions were not found</Typography>;
  }

  const segmenter = new Intl.Segmenter()


  
  return (
    <Container maxWidth="md" className="handwriting-page-container">
      <Box className="handwriting-canvas-wrapper">
        {/* <HandwritingCanvas /> */}
      </Box>
    </Container>
  );
};

export default HandwritingPage;
