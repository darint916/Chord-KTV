import React, { useRef, useState } from 'react';
import HandwritingCanvas from '../../components/HandwritingCanvas/HandwritingCanvas';
import {
  Container,
  Box,
  Typography,
  Button,
  Grid,
  List,
  ListItem,
  ListItemText,
  ListItemButton,
  Stack,
  Chip,
} from '@mui/material';
import { useSong } from '../../contexts/SongContext';
import './HandwritingPage.scss';
import { LanguageCode } from '../../api';
import { useNavigate } from 'react-router-dom';

const HandwritingPage: React.FC = () => {
  const { song, quizQuestions } = useSong();
  const [currentWordIndex, setCurrentWordIndex] = useState(0);
  const [completedWords, setCompletedWords] = useState<number[]>([]);
  const [currentWordCompleted, setCurrentWordCompleted] = useState(false);
  const handwritingCanvasRef = useRef<{ clearCanvas: () => void }>(null);
  const [quizCompleted, setQuizCompleted] = useState(false);
  const navigate = useNavigate();

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
      .map(segment => segment.segment)
      .filter(segment => !/[A-Za-z]/.test(segment));

    if (segments.length === 0) { return ''; }

    return segments.reduce((longest, current) =>
      current.length > longest.length ? current : longest
    );
  };

  const wordsToPractice = quizQuestions
    .map(q => getLongestWord(q.lyricPhrase ?? ''))
    .filter(word => word.length > 0);

  const currentWord = wordsToPractice[currentWordIndex % wordsToPractice.length];
  const allWordsCompleted = completedWords.length >= wordsToPractice.length;
  if (allWordsCompleted) {
    setQuizCompleted(true);
  }

  const handleWordCompletionAttempt = (isSuccess: boolean) => {
    if (isSuccess) {
      const newCompletedWords = [...new Set([...completedWords, currentWordIndex])];
      setCompletedWords(newCompletedWords);
      setCurrentWordCompleted(true);
    }
  };

  const moveToNextWord = () => {
    if (handwritingCanvasRef.current) {
      handwritingCanvasRef.current.clearCanvas();
    }

    // Find the next uncompleted word index
    let nextIndex = currentWordIndex;
    do {
      nextIndex = (nextIndex + 1) % wordsToPractice.length;
    } while (completedWords.includes(nextIndex) && completedWords.length < wordsToPractice.length);

    setCurrentWordIndex(nextIndex);
    setCurrentWordCompleted(false);
  };

  const resetQuiz = () => {
    if (handwritingCanvasRef.current) {
      handwritingCanvasRef.current.clearCanvas();
    }
    setCurrentWordIndex(0);
    setCompletedWords([]);
    setCurrentWordCompleted(false);
    setQuizCompleted(false);
  };

  const handleWordSelect = (index: number) => {
    setCurrentWordIndex(index);
    if (handwritingCanvasRef.current) {
      handwritingCanvasRef.current.clearCanvas();
    }
    setCurrentWordCompleted(false);
  };

  const completeQuiz = () => {
    setQuizCompleted(true);
  };

  const handleBackToHome = () => {
    navigate('/');
  };


  if (quizCompleted) {
    const completedWordsList = completedWords.map(index => wordsToPractice[index]);

    return (
      <Container maxWidth="md" className="handwriting-page-container">
        <Typography variant="h4" gutterBottom align="center">
          Handwriting Practice Completed!
        </Typography>
        <Typography variant="h6" gutterBottom align="center">
          You practiced {completedWords.length} out of {wordsToPractice.length} words successfully.
        </Typography>

        <Box sx={{ mt: 4, mb: 4 }}>
          <Typography variant="h6" gutterBottom>
            Completed Words:
          </Typography>
          <Stack direction="row" spacing={1} useFlexGap flexWrap="wrap">
            {completedWordsList.length > 0 ? (
              completedWordsList.map((word, index) => (
                <Chip
                  key={index}
                  label={word}
                  color="success"
                  variant="outlined"
                  sx={{ mb: 1 }}
                />
              ))
            ) : (
              <Typography variant="body2" color="text.secondary">
                No words completed yet
              </Typography>
            )}
          </Stack>
        </Box>

        <Box textAlign="center" mt={4} display="flex" gap={2} justifyContent={'center'}>
          <Button
            variant="contained"
            color="primary"
            onClick={resetQuiz}
            sx={{ mt: 2 }}
          >
            Practice Again
          </Button>
          <Button
            variant="contained"
            color="primary"
            onClick={handleBackToHome}
            sx={{ mt: 2 }}
          >
            Back to Home
          </Button>
        </Box>
      </Container>
    );
  }


  return (
    <Container maxWidth="lg" className="handwriting-page-container">
      <Grid container spacing={3} className="grid-parent">
        {/* Left column - blank for now */}
        <Grid item xs={12} md={3}>
        </Grid>

        {/* Middle column - Canvas */}
        <Grid item xs={12} md={6} className="grid-item">
          <div className="handwriting-canvas-parent">
            <Typography variant="h5" gutterBottom align="center">
              Handwriting Practice
            </Typography>
            <Typography variant="h6" gutterBottom align="center">
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

            <Box sx={{ display: 'flex', justifyContent: 'center', gap: 2, mb: 6, mt: 1 }}>
              {currentWordCompleted ? (
                <Button
                  variant="contained"
                  color="success"
                  onClick={moveToNextWord}
                >
                  Next Word
                </Button>
              ) : (
                <Button
                  variant="outlined"
                  color="error"
                  onClick={moveToNextWord}
                >
                  Skip Word
                </Button>
              )}
              <Button
                variant="outlined"
                color="secondary"
                onClick={completeQuiz}
              >
                Complete Quiz
              </Button>
            </Box>
          </div>
        </Grid>

        {/* Right column - Word list */}
        <Grid item xs={12} md={3}>
          <div className="grid-item">
            <Typography variant="h6" gutterBottom>
              Words to Practice
            </Typography>
            <List dense={true}>
              {wordsToPractice.map((word, index) => (
                <ListItem
                  key={index}
                  disablePadding
                  sx={{
                    mb: 0.5,
                    '& .MuiListItemButton-root': {
                      borderRadius: 1,
                      backgroundColor: completedWords.includes(index)
                        ? 'success.light'
                        : currentWordIndex === index
                          ? 'action.selected'
                          : 'transparent',
                      '&:hover': {
                        backgroundColor: completedWords.includes(index)
                          ? 'success.light'
                          : 'action.hover',
                      }
                    }
                  }}
                >
                  <ListItemButton
                    onClick={() => handleWordSelect(index)}
                    selected={currentWordIndex === index}
                  >
                    <ListItemText
                      primary={word}
                      secondary={completedWords.includes(index) ? '✓ Completed' : ''}
                      sx={{
                        color: completedWords.includes(index) ? 'success.dark' : 'text.primary',
                        '& .MuiListItemText-secondary': {
                          color: 'success.main',
                          fontWeight: 'bold'
                        }
                      }}
                    />
                  </ListItemButton>
                </ListItem>
              ))}
            </List>
          </div>
        </Grid>
      </Grid>
    </Container>
  );
};

export default HandwritingPage;
