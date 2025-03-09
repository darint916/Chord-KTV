import React, { useEffect, useState } from 'react';
import { Box, Typography, Button } from '@mui/material';
import { quizApi } from '../../api/apiClient';
import { useSong } from '../../contexts/SongContext';
import Quiz from 'react-quiz-component';
import { useNavigate } from 'react-router-dom';
import './QuizComponent.scss';

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

const QuizComponent: React.FC<{ songId: string }> = ({ songId }) => {
  const [quizData, setQuizData] = useState<QuizData | null>(null);
  const { quizQuestions, setQuizQuestions } = useSong();
  const [quizCompleted, setQuizCompleted] = useState(false);
  const navigate = useNavigate();

  useEffect(() => {
    const fetchQuestions = async () => {
      if (quizQuestions && quizQuestions.length > 0) {
        return;
      }
      try {
        const response = await quizApi.apiQuizRomanizationGet({ songId });
        setQuizQuestions(response.questions ?? []);
      } catch {
        setQuizQuestions([]);
      }
    };

    fetchQuestions();
  }, [songId, quizQuestions, setQuizQuestions]);

  useEffect(() => {
    if (!quizQuestions || quizQuestions.length === 0) {return;}

    const formattedQuiz: QuizData = {
      quizTitle: 'Lyrics Romanization Quiz',
      quizSynopsis: 'Select the correct romanized version of the lyric.',
      progressBarColor: '#9de1f6',
      nrOfQuestions: quizQuestions.length.toString(),
      questions: quizQuestions.map((question) => {
        return {
          question: question.lyricPhrase,
          questionType: 'text',
          answerSelectionType: 'single',
          answers: question.options,
          correctAnswer: ((question.correctOptionIndex ?? 0) + 1).toString(),
          point: '2',
          messageForCorrectAnswer: 'Correct! Click Next to continue.',
          messageForIncorrectAnswer: 'Incorrect, click Next to continue.'
        };
      }),
    };

    setQuizData(formattedQuiz);
  }, [quizQuestions]);

  if (!quizData || !quizData.questions || quizData.questions.length === 0) {
    return <Typography variant="h5">Loading quiz questions...</Typography>;
  }

  const handleQuizComplete = () => {
    setQuizCompleted(true); // Set quiz as completed
  };

  const handleBackToHome = () => {
    navigate('/'); // Navigate to home page
  };

  return (
    <div>
      <Box className="quiz-container">
        <Quiz
          quiz={quizData}
          shuffle={true}
          shuffleAnswer={true}
          showInstantFeedback={true}
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
