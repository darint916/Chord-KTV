import React, { useEffect, useState } from 'react';
import { Box, Button, Typography, FormControl, RadioGroup, FormControlLabel, Radio } from '@mui/material';
import { quizApi } from '../../api/apiClient';
import { useSong } from '../../contexts/SongContext';
import './QuizComponent.scss';

const QuizComponent: React.FC<{ songId: string }> = ({ songId }) => {
  const [currentQuestionIndex, setCurrentQuestionIndex] = useState<number>(0);
  const [selectedAnswer, setSelectedAnswer] = useState<string | null>(null);
  const [score, setScore] = useState<number>(0);
  const [quizFinished, setQuizFinished] = useState<boolean>(false);
  const { quizQuestions, setQuizQuestions } = useSong();

  useEffect(() => {
    // Fetch the quiz questions from the backend API if not stored in context
    const fetchQuestions = async () => {
      if (quizQuestions && quizQuestions.length > 0) return;

      try {
        const response = await quizApi.apiQuizRomanizationGet({
          'songId': songId
        });
        const fetchedQuestions = response.questions ?? [];
        setQuizQuestions(fetchedQuestions); 

      } catch {
        return <Typography variant="h5">Error fetching quiz questions</Typography>;
      }
    };

    fetchQuestions();
  }, [songId, quizQuestions, setQuizQuestions]);

  const handleAnswerChange = (event: React.ChangeEvent<HTMLInputElement>) => {
    setSelectedAnswer(event.target.value);
  };

  const handleNextQuestion = () => {
    if (currentQuestion.options && currentQuestion.correctOptionIndex && 
      selectedAnswer === currentQuestion.options[currentQuestion.correctOptionIndex]) {
      setScore(score + 1);
    }
    setSelectedAnswer(null);
    setCurrentQuestionIndex(currentQuestionIndex + 1);
  };

  const handleFinishQuiz = () => {
    if (currentQuestion.options && currentQuestion.correctOptionIndex && 
      selectedAnswer === currentQuestion.options[currentQuestion.correctOptionIndex]) {
      setScore(score + 1);
    }
    setQuizFinished(true);
  };

  if (!quizQuestions) {
    return <Typography variant="h5">Loading quiz questions...</Typography>;
  }

  if (quizFinished) {
    return (
      <Box>
        <Typography variant="h5">Quiz Finished!</Typography>
        <Typography variant="h6">Your score: {score}/{quizQuestions.length}</Typography>
      </Box>
    );
  }

  const currentQuestion = quizQuestions[currentQuestionIndex];

  return (
    <div>
      <Typography variant='h5'>Select the correct romanized version of the lyric below:</Typography>
      <Box className="quiz-container">
        <div className="quiz-content">
          <Typography variant="h4">{currentQuestion.lyricPhrase}</Typography>
          {currentQuestion.options && currentQuestion.options.length > 0 ? (
            <FormControl component="fieldset">
              <RadioGroup value={selectedAnswer} onChange={handleAnswerChange}>
                {currentQuestion.options.map((option, index) => (
                  <FormControlLabel
                    key={index}
                    value={option}
                    control={<Radio />}
                    label={option}
                    className="options"
                  />
                ))}
              </RadioGroup>
            </FormControl>
          ) : (
            <Typography variant="body1">No options available for this question.</Typography>
          )} 
          {currentQuestionIndex < quizQuestions.length - 1 ? (
            <Button onClick={handleNextQuestion} variant="contained" color="primary" disabled={!selectedAnswer}>
              Next Question
            </Button>
          ) : (
            <Button onClick={handleFinishQuiz} variant="contained" color="secondary" disabled={!selectedAnswer}>
              Finish Quiz
            </Button>
          )}
        </div>
      </Box>
    </div>
  );
};

export default QuizComponent;
