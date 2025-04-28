import React, { useRef, useState, useEffect } from 'react';
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
import { LanguageCode, QuizQuestionDto } from '../../api';
import { useNavigate, useSearchParams } from 'react-router-dom';
import { quizApi } from '../../api/apiClient';

const HandwritingPage: React.FC = () => {
  const { song, quizQuestions, setQuizQuestions } = useSong();
  const [currentWordIndex, setCurrentWordIndex] = useState(0);
  const [completedWords, setCompletedWords] = useState<number[]>([]);
  const [currentWordCompleted, setCurrentWordCompleted] = useState(false);
  const handwritingCanvasRef = useRef<{ clearCanvas: () => void }>(null);
  const [quizCompleted, setQuizCompleted] = useState(false);
  const navigate = useNavigate();

  /* ─────  URL params  ───── */
  const [searchParams] = useSearchParams();
  const urlSongId = searchParams.get('id');

  /* fetch guard / loading flag */
  const [loadingQuestions, setLoadingQuestions] = useState(false);

  useEffect(() => {
    const effectiveSongId =
      urlSongId || (song?.id && song.id.trim() !== '' ? song.id : null);

    if (!effectiveSongId) { return; }               // nothing to fetch with
    if (quizQuestions && quizQuestions.length) { return; } // already loaded

    (async () => {
      try {
        setLoadingQuestions(true);
        const resp = await quizApi.apiQuizRomanizationPost({
          songId: effectiveSongId,
        });
        setQuizQuestions(resp.questions ?? []);
      } catch {
        setQuizQuestions([]);
      } finally {
        setLoadingQuestions(false);
      }
    })();
  }, [urlSongId, song?.id, quizQuestions, setQuizQuestions]);

  if (loadingQuestions || !quizQuestions) {
    return <Typography variant="h5">Loading handwriting quiz…</Typography>;
  }
  if (quizQuestions.length === 0) {
    return (
      <Typography variant="h5">
        Error: Could not load handwriting quiz questions
      </Typography>
    );
  }

  if (!song || !song.geniusMetaData) {
    return <Typography variant="h5">Error: Song data is undefined or corrupted</Typography>;
  }

  /* ───── 2.  Robust segmenter (fallback to 'en' if UNK/empty) ───── */
  const languageForSegmentation =
    song.geniusMetaData.language && song.geniusMetaData.language !== 'UNK'
      ? song.geniusMetaData.language
      : 'en';

  const segmenter = new Intl.Segmenter(languageForSegmentation, {
    granularity: 'word',
  });

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

  /* ───── helper: pick a phrase even for AUDIO quizzes ───── */
  const extractPhrase = (q: QuizQuestionDto): string => {
    if (q.lyricPhrase && q.lyricPhrase.trim().length) {
      return q.lyricPhrase;
    }
    const idx = q.correctOptionIndex ?? 0;
    return q.options?.[idx] ?? '';
  };

  const wordsToPractice = quizQuestions
    .map(q => getLongestWord(extractPhrase(q)))
    .filter(word => word.length > 0);

  /* guard: no usable words after extraction */
  if (wordsToPractice.length === 0) {
    return (
      <Typography variant="h5">
        Error: Could not extract practice words from the quiz data.
      </Typography>
    );
  }
  const currentWord = wordsToPractice[currentWordIndex % wordsToPractice.length];

  useEffect(() => {
    if (completedWords.length >= wordsToPractice.length && wordsToPractice.length > 0) {
      setQuizCompleted(true);
    }
  }, [completedWords, wordsToPractice.length]);


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
