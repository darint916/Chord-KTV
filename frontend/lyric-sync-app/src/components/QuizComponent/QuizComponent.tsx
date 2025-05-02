import React, { useEffect, useState } from 'react';
import { Box, Typography, Button, IconButton, Slider, Alert } from '@mui/material';
import PlayArrowIcon from '@mui/icons-material/PlayArrow';
import StopIcon from '@mui/icons-material/Stop';
import { quizApi, userActivityApi, handwritingApi } from '../../api/apiClient';
import { useSong } from '../../contexts/SongContext';
import Quiz from 'react-quiz-component';
import { useNavigate } from 'react-router-dom';
import './QuizComponent.scss';
import AudioSnippetPlayer from '../AudioSnippetPlayer/AudioSnippetPlayer';
import { parseTimeSpan } from '../../utils/timeUtils';
import type { QuizResponseDto, LanguageCode, UserQuizResultDto, QuizQuestionDto } from '../../api/models';
import CircularProgress from '@mui/material/CircularProgress';

interface QuizData {
  quizTitle: string;
  quizSynopsis: string;
  progressBarColor: string;
  nrOfQuestions: string;
  questions: {
    question: string | null | undefined;
    questionType: string;
    answerSelectionType: string;
    answers: string[] | null | undefined;
    correctAnswer: string;
    point: string;
    messageForCorrectAnswer: string;
    messageForIncorrectAnswer: string;
  }[];
}

const QuizComponent: React.FC<{ songId: string, lyricsOffset?: number }> = ({ songId, lyricsOffset = 0 }) => {
  const [quizData, setQuizData] = useState<QuizData | null>(null);
  const [quizId, setQuizId] = useState<string>('');
  const { quizQuestions, setQuizQuestions, song, setHandwritingQuizQuestions } = useSong();
  const [quizCompleted, setQuizCompleted] = useState(false);
  const [isLoading, setIsLoading] = useState(false);
  const [selectedQuizType, setSelectedQuizType] = useState<'romanization' | 'audio' | null>(null);
  const navigate = useNavigate();
  const [isPlaying, setIsPlaying] = useState(false);
  const [currentQuestionIndex, setCurrentQuestionIndex] = useState(0);
  const [isRedirecting, setIsRedirecting] = useState(false);
  const [handwritingDifficulty, setHandwritingDifficulty] = useState<number>(5);
  const [error, setError] = useState('');

  const handleStartQuiz = async (quizType: 'romanization' | 'audio') => {
    setIsLoading(true);
    setSelectedQuizType(quizType);

    // build your POST body
    const requestBody = {
      songId,           // required
      useCachedQuiz: false,
      difficulty: 3,
      numQuestions: 5
    };

    try {
      let response: QuizResponseDto;

      if (quizType === 'romanization') {
        // note the wrapper property name!
        response = await quizApi.apiQuizRomanizationPost({
          quizRequestDto: requestBody
        });
      } else {
        response = await quizApi.apiQuizAudioPost({
          quizRequestDto: requestBody
        });
      }

      setQuizQuestions(response.questions ?? []);
      setQuizId(response.quizId ?? '');
    } catch {
      // console.error('failed to fetch quiz', e);
      setQuizQuestions([]);
    } finally {
      setIsLoading(false);
    }
  };

  // Build the quizData once the quizQuestions become available.
  useEffect(() => {
    if (!quizQuestions || quizQuestions.length === 0) {
      setQuizData(null);
      return;
    }

    const formattedQuiz: QuizData = {
      quizTitle: selectedQuizType === 'audio' ? 'Audio Quiz' : 'Lyrics Romanization Quiz',
      quizSynopsis: selectedQuizType === 'audio'
        ? 'Listen to the snippet and select the correct lyric.'
        : 'Select the correct romanized version of the lyric.',
      progressBarColor: '#9de1f6',
      nrOfQuestions: quizQuestions.length.toString(),
      questions: quizQuestions.map((question) => ({
        question: ' ' + question.lyricPhrase,
        questionType: 'text',
        answerSelectionType: 'single',
        answers: question.options,
        correctAnswer: ((question.correctOptionIndex ?? 0) + 1).toString(),
        point: '2',
        messageForCorrectAnswer: 'Correct, well done!',
        messageForIncorrectAnswer: 'Incorrect, click Next to continue.'
      }))
    };

    setQuizData(formattedQuiz);
  }, [quizQuestions, selectedQuizType]);

  useEffect(() => {
    setQuizQuestions([]);
  }, []);

  if (!quizData) {
    return (
      <Box display="flex" justifyContent="center" alignItems="center" gap={2} flexDirection="column">
        <Typography variant="h6">Choose Quiz Type</Typography>
        <Box display="flex" gap={2}>
          <Button
            variant="contained"
            color="primary"
            onClick={() => handleStartQuiz('romanization')}
            disabled={isLoading}
            startIcon={
              isLoading && selectedQuizType === 'romanization' ? (
                <CircularProgress size={20} color="inherit" />
              ) : undefined
            }
          >
            {isLoading && selectedQuizType === 'romanization'
              ? 'Preparing...'
              : 'Start Romanization Quiz'}
          </Button>

          <Button
            variant="outlined"
            color="secondary"
            onClick={() => handleStartQuiz('audio')}
            disabled={isLoading}
            startIcon={
              isLoading && selectedQuizType === 'audio' ? (
                <CircularProgress size={20} color="inherit" />
              ) : undefined
            }
          >
            {isLoading && selectedQuizType === 'audio'
              ? 'Preparing...'
              : 'Do Audio Quiz Instead'}
          </Button>
        </Box>
      </Box>
    );
  }

  const handleQuizComplete = async (quizResult: { numberOfCorrectAnswers?: number; score?: number; questionSummary?: Array<{ answers: string[]; correctAnswer: string; }>; }) => {
    setQuizCompleted(true);
    setIsPlaying(false);

    const score = quizResult.numberOfCorrectAnswers ?? quizResult.score ?? 0;

    let correctAnswers: string[] = [];

    if (Array.isArray(quizQuestions) && quizQuestions.length > 0) {
      if (selectedQuizType === 'romanization') {
        // For romanization quizzes, use the original lyric phrase
        correctAnswers = (quizQuestions as QuizQuestionDto[])
          .map((q, index) => {
            const userAnswerIndex = parseInt(quizResult.questionSummary?.[index]?.answers[0] ?? '0') - 1;
            const correctAnswerIndex = parseInt(quizResult.questionSummary?.[index]?.correctAnswer ?? '0') - 1;
            const isCorrect = userAnswerIndex === correctAnswerIndex;
            return isCorrect ? q.lyricPhrase || '' : '';
          })
          .filter((ans): ans is string => !!ans);
      } else if (selectedQuizType === 'audio') {
        // For audio quizzes, use the correct answer from the answer choices
        correctAnswers = (quizQuestions as QuizQuestionDto[])
          .map((q, index) => {
            const userAnswerIndex = parseInt(quizResult.questionSummary?.[index]?.answers[0] ?? '0') - 1;
            const correctAnswerIndex = parseInt(quizResult.questionSummary?.[index]?.correctAnswer ?? '0') - 1;
            const isCorrect = userAnswerIndex === correctAnswerIndex;
            const idx = q.correctOptionIndex ?? 0;
            return isCorrect ? q.options?.[idx] ?? '' : '';
          })
          .filter((ans): ans is string => !!ans);
      }
    }

    // Debug log to verify we're getting all answers
    // console.log(`Quiz completed with ${correctAnswers.length}/${quizQuestions?.length || 0} answers collected`);

    const language: LanguageCode =
      (song?.geniusMetaData?.language ?? 'UNK') as LanguageCode;

    try {
      await userActivityApi.apiUserActivityQuizPost({
        userQuizResultDto: {
          quizId,
          score,
          language,
          correctAnswers,
        } as UserQuizResultDto,
      });
    } catch {
      // console.error('Failed to log quiz result', err);
    }
  };

  const handleBackToHome = () => {
    navigate('/');
  };

  const handleStartHandwritingQuiz = async () => {
    if (!song?.id || !quizQuestions || quizQuestions.length === 0) { return; }

    setIsRedirecting(true);

    try {
      const phrases = quizQuestions.map(q => {
        if (q.lyricPhrase?.trim()) { return q.lyricPhrase; }
        const idx = q.correctOptionIndex ?? 0;
        return q.options?.[idx] ?? '';
      });

      const translationResp = await handwritingApi.apiHandwritingOcrTranslateGet({
        phrases,
        languageCode: song.geniusMetaData?.language as LanguageCode,
        difficulty: handwritingDifficulty
      });

      setHandwritingQuizQuestions(translationResp);
      navigate('/handwriting-quiz');
    } catch (err) {
      const errorMessage = err instanceof Error
        ? `Failed to fetch handwriting quiz phrases: ${err.message}`
        : 'Failed to fetch handwriting quiz phrases';
      setError(errorMessage);
      setIsRedirecting(false);
    }
  };


  const handleTogglePlay = () => {
    setIsPlaying(p => !p);
  };

  if (!quizQuestions || !quizQuestions[currentQuestionIndex]) {
    return <Typography variant="h5">Invalid quiz state. Please try again.</Typography>;
  }


  // log on every render what the computed timestamps are
  const rawStart = parseTimeSpan(quizQuestions[currentQuestionIndex]?.startTimestamp);
  const rawEnd = parseTimeSpan(quizQuestions[currentQuestionIndex]?.endTimestamp);
  const startSec = Math.max(0, rawStart - lyricsOffset);
  const endSec = Math.max(startSec, rawEnd - lyricsOffset); // keep non-negative & ordered

  const videoId =
    song?.youTubeId ??
    song?.alternateYoutubeIds?.[0] ??
    'dQw4w9WgXcQ'; // fallback

  return (
    <div>
      {error && (
        <Box mx="auto" width="fit-content" mt={2}>
          <Alert severity="error" onClose={() => setError('')} variant="filled">
            {error}
          </Alert>
        </Box>
      )}
      {quizCompleted && (
        <Box display="flex" flexDirection="column" alignItems="center" gap={2} mt={2}>
          <Box width={300}>
            <Typography gutterBottom className="difficulty-text">
              Handwriting Quiz Difficulty: {handwritingDifficulty}
            </Typography>
            <Slider
              value={handwritingDifficulty}
              min={1}
              max={10}
              step={1}
              valueLabelDisplay="auto"
              onChange={(_, value) => setHandwritingDifficulty(value as number)}
              marks={[
                { value: 1, label: '1' },
                { value: 2, label: '2' },
                { value: 3, label: '3' },
                { value: 4, label: '4' },
                { value: 5, label: '5' },
                { value: 6, label: '6' },
                { value: 7, label: '7' },
                { value: 8, label: '8' },
                { value: 9, label: '9' },
                { value: 10, label: '10' }
              ]}
            />
          </Box>

          <Button
            variant="contained"
            color="primary"
            onClick={handleStartHandwritingQuiz}
            disabled={isRedirecting}
            startIcon={isRedirecting ? <CircularProgress size={20} color="inherit" /> : undefined}
          >
            {isRedirecting ? 'Preparing...' : 'Start Handwriting Quiz'}
          </Button>
        </Box>

      )}
      <Box className="quiz-container">
        {selectedQuizType === 'audio' && !quizCompleted && (
          <>
            <AudioSnippetPlayer
              key={`audio-${currentQuestionIndex}`}
              videoId={videoId}
              startTime={startSec}
              endTime={endSec}
              play={isPlaying}
              onEnded={() => setIsPlaying(false)}
            />
            <IconButton
              onClick={handleTogglePlay}
              color="primary"
              className="quiz-icon-button"
              sx={{ mb: 2 }}
            >
              {isPlaying ? <StopIcon /> : <PlayArrowIcon />}
            </IconButton>
          </>
        )}
        <Quiz
          quiz={quizData}
          shuffle={selectedQuizType === 'audio' ? false : true}
          shuffleAnswer={true}
          showInstantFeedback={true}
          onQuestionSubmit={() => {
            setCurrentQuestionIndex(prev => {
              const next = prev + 1;
              return next < quizQuestions.length ? next : prev;
            });
            setIsPlaying(false);      // stop current snippet
          }}
          onComplete={handleQuizComplete}
        />
      </Box>
      {quizCompleted && (
        <Box marginTop={2}>
          <Button variant="contained" color="primary" onClick={handleBackToHome}>
            Back to Home
          </Button>
        </Box>
      )}
    </div>
  );
};

export default QuizComponent;
