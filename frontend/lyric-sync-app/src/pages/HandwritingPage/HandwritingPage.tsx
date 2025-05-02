import React, { useRef, useState, useEffect } from 'react';
import HandwritingCanvas from '../../components/HandwritingCanvas/HandwritingCanvas';
import {
  Container,
  Box,
  Typography,
  Button,
  List,
  ListItem,
  ListItemText,
  ListItemButton,
  Stack,
  Chip,
  Divider,
  Snackbar,
  Alert
} from '@mui/material';
import Grid from '@mui/material/Grid2';
import { useSong } from '../../contexts/SongContext';
import './HandwritingPage.scss';
import { LanguageCode, UserHandwritingResultDto } from '../../api';
import { useNavigate } from 'react-router-dom';
import { userActivityApi, handwritingApi } from '../../api/apiClient';
import { IconButton, Tooltip } from '@mui/material';
import SkipNextIcon from '@mui/icons-material/SkipNext';
import { Publish } from '@mui/icons-material';

const HandwritingPage: React.FC = () => {
  const { song, handwritingQuizQuestions } = useSong();
  const [currentWordIndex, setCurrentWordIndex] = useState(0);
  const [completedWords, setCompletedWords] = useState<number[]>([]);
  const [currentWordCompleted, setCurrentWordCompleted] = useState(false);
  const [quizCompleted, setQuizCompleted] = useState(false);
  const navigate = useNavigate();
  const [snackbarOpen, setSnackbarOpen] = useState(false);
  const [snackbarSeverity, setSnackbarSeverity] = useState<'success' | 'error' | 'info'>('info');
  const [snackbarMessage, setSnackbarMessage] = useState('');
  const practicePhrases = handwritingQuizQuestions?.phrases ?? [];
  const handwritingCanvasRef = useRef<{
    clearCanvas: () => void;
    getImageData: () => string | null;
      }>(null);

  const handleSubmit = async () => {
    if (!handwritingCanvasRef.current) { return; }
    const imageData = handwritingCanvasRef.current.getImageData?.();
    if (!imageData) { return; }

    try {
      const response = await handwritingApi.apiHandwritingOcrPost({
        handwritingCanvasRequestDto: {
          image: imageData,
          language: song?.geniusMetaData?.language as LanguageCode,
          expectedMatch: currentWord.original,
        },
      });

      const match = response.matchPercentage || 0;
      const recognized = response.recognizedText || '';

      if (match === 100) {
        setSnackbarSeverity('success');
        setSnackbarMessage('Perfect match! Good job!');
      } else {
        setSnackbarSeverity('info');
        setSnackbarMessage(`Partial match: ${match}% — Recognized: "${recognized}"`);
      }

      setSnackbarOpen(true);
      handleWordCompletionAttempt(match);
    } catch {
      setSnackbarSeverity('error');
      setSnackbarMessage('Recognition failed. Please try again.');
      setSnackbarOpen(true);
      handleWordCompletionAttempt(-1);
    }
  };

  if (!handwritingQuizQuestions?.phrases || handwritingQuizQuestions.phrases.length === 0) {
    return (
      <Typography variant="h5">
        Error: Could not load handwriting quiz questions
      </Typography>
    );
  }

  if (!song || !song.geniusMetaData) {
    return <Typography variant="h5">Error: Song data is undefined or corrupted</Typography>;
  }

  const currentWord = practicePhrases[currentWordIndex % practicePhrases.length];

  useEffect(() => {
    if (completedWords.length >= practicePhrases.length && practicePhrases.length > 0) {
      setQuizCompleted(true);
    }
  }, [completedWords, practicePhrases.length]);

  const handleWordCompletionAttempt = (raw: number): void => {
    const matchPct = raw;

    // compute 0–5
    const scoreOutOf5 = Math.round((matchPct / 100) * 5);
    const language = (song?.geniusMetaData?.language ?? 'UNK') as LanguageCode;
    const dto: UserHandwritingResultDto = {
      language,
      score: scoreOutOf5,
      wordTested: currentWord.original,
    };

    if (scoreOutOf5 > 0) {
      userActivityApi
        .apiUserActivityHandwritingPost({ userHandwritingResultDto: dto })
        .catch(() => { });
    }

    // locally mark complete at ≥98%
    if (matchPct >= 98) {

      userActivityApi.apiUserActivityLearnedWordPost({
        learnedWordDto: {
          language,
          word: currentWord.original,
        }
      })
        .catch(() => { });

      setCompletedWords(prev => [...new Set([...prev, currentWordIndex])]);
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
      nextIndex = (nextIndex + 1) % practicePhrases.length;
    } while (completedWords.includes(nextIndex) && completedWords.length < practicePhrases.length);

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
    const completedWordsList = completedWords.map(index => practicePhrases[index]);

    return (
      <Container maxWidth="md" className="handwriting-page-container">
        <Typography variant="h4" gutterBottom align="center">
          Handwriting Practice Completed!
        </Typography>
        <Typography variant="h6" gutterBottom align="center">
          You practiced {completedWords.length} out of {practicePhrases.length} words successfully.
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
                  label={word.original}
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
      <Box display="flex" justifyContent="center" alignItems="center" mb={1}>
        <Typography variant="h4" gutterBottom align="center" fontWeight="bold" component="h1">
          Handwriting Practice
        </Typography>
      </Box>
      <Typography variant="h5" gutterBottom align="center" fontWeight="bold" component="h1">
        Current word: {currentWord.original}
      </Typography>
      {currentWord.romanized && (
        <Typography variant="h6" align="center" className="italic-title">
          Romanization: {currentWord.romanized}
        </Typography>
      )}

      {currentWord.translated && (
        <Typography variant="h6" align="center" className="italic-title">
          Translation: {currentWord.translated}
        </Typography>
      )}
      <Grid container spacing={3} className="grid-parent">
        <Grid size={8} className="grid-item">
          <Box className="handwriting-canvas-wrapper">
            <HandwritingCanvas
              ref={handwritingCanvasRef}
              expectedText={currentWord.original ?? ''}
              selectedLanguage={song.geniusMetaData.language as LanguageCode}
              onComplete={handleWordCompletionAttempt}
            />
          </Box>
        </Grid>

        {/* Right column - Word list */}
        <Grid size={4}>
          <div className="grid-item">
            <Typography variant="h6" fontWeight="bold" component="h1" align='center'>
              Words to Practice
            </Typography>
            <Divider variant="fullWidth" className="list-divider" />
            <List dense={true} disablePadding>
              {practicePhrases.map((word, index) => (
                <ListItem
                  key={index}
                  disablePadding
                >
                  <ListItemButton
                    onClick={() => handleWordSelect(index)}
                    selected={currentWordIndex === index}
                  >
                    <ListItemText
                      primary={word.original}
                      secondary={completedWords.includes(index) ? '✓ Completed' : ''}
                      sx={{
                        color: completedWords.includes(index) ? 'success.dark' : 'text.primary',
                        '& .MuiListItemText-secondary': {
                          color: 'success.main',
                          fontWeight: 'bold'
                        },
                        '& .MuiListItemText-primary': {
                          fontWeight: 'bold'
                        }
                      }}
                    />
                  </ListItemButton>
                </ListItem>
              ))}
            </List>
            <Stack direction="row" justifyContent="center" spacing={2} mt={2}>
              <Tooltip title={currentWordCompleted ? 'Next Word' : 'Skip Word'}>
                <IconButton
                  onClick={moveToNextWord}
                  color={currentWordCompleted ? 'success' : 'error'}
                  size="large"
                >
                  <SkipNextIcon />
                </IconButton>
              </Tooltip>

              <Button variant="contained" className="submit-handwriting-button" onClick={handleSubmit}>
                Submit Word
              </Button>

              <Tooltip title="Finish Quiz">
                <IconButton onClick={completeQuiz} color="secondary" size="large">
                  <Publish />
                </IconButton>
              </Tooltip>
            </Stack>

          </div>
        </Grid>
      </Grid>
      <Snackbar
        open={snackbarOpen}
        autoHideDuration={4000}
        onClose={() => setSnackbarOpen(false)}
        anchorOrigin={{ vertical: 'bottom', horizontal: 'center' }}
      >
        <Alert severity={snackbarSeverity} onClose={() => setSnackbarOpen(false)} variant="filled">
          {snackbarMessage}
        </Alert>
      </Snackbar>
    </Container>
  );
};

export default HandwritingPage;
