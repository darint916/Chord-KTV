import React from 'react';
import HandwritingCanvas from '../../components/HandwritingCanvas/HandwritingCanvas';
import { Container, Box } from '@mui/material';
import './HandwritingPage.scss';

const HandwritingPage: React.FC = () => {
  return (
    <Container maxWidth="md" className="handwriting-page-container">
      <Box className="handwriting-canvas-wrapper">
        <HandwritingCanvas />
      </Box>
    </Container>
  );
};

export default HandwritingPage;
