import React, { useEffect, useState } from 'react';
import { Box, Button, Typography, FormControl, RadioGroup, FormControlLabel, Radio } from '@mui/material';
import { quizApi } from '../../api/apiClient';
import { QuizQuestionDto } from '../../api';
import './QuizComponent.scss';

const QuizComponent: React.FC<{ songId: string }> = ({ songId }) => {
  const [questions, setQuestions] = useState<QuizQuestionDto[]>([]);
  const [currentQuestionIndex, setCurrentQuestionIndex] = useState<number>(0);
  const [selectedAnswer, setSelectedAnswer] = useState<string | null>(null);
  const [score, setScore] = useState<number>(0);
  const [quizFinished, setQuizFinished] = useState<boolean>(false);

  useEffect(() => {
    // Fetch the quiz questions from the backend API
    const fetchQuestions = async () => {
      try {
        const response = await quizApi.apiQuizRomanizationGet({
          'songId': songId
        });
        setQuestions(response.questions ?? []);
      } catch {
        return <Typography variant="h5">Error fetching quiz questions</Typography>;
      }
    };

    fetchQuestions();
  }, [songId]);

  const handleAnswerChange = (event: React.ChangeEvent<HTMLInputElement>) => {
    setSelectedAnswer(event.target.value);
  };

  const handleNextQuestion = () => {
    if (selectedAnswer === questions[currentQuestionIndex].correctOptionIndex) {
      setScore(score + 1);
    }
    setSelectedAnswer(null);
    setCurrentQuestionIndex(currentQuestionIndex + 1);
  };

  const handleFinishQuiz = () => {
    if (selectedAnswer === questions[currentQuestionIndex].correctOptionIndex) {
      setScore(score + 1);
    }
    setQuizFinished(true);
  };

  if (quizFinished) {
    return (
      <Box>
        <Typography variant="h5">Quiz Finished!</Typography>
        <Typography variant="h6">Your score: {score}/{questions.length}</Typography>
      </Box>
    );
  }

  if (questions.length === 0) {
    return <Typography variant="h5">Loading quiz questions...</Typography>;
  }

  const currentQuestion = questions[currentQuestionIndex];

  return (
    <Box>
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
              />
            ))}
          </RadioGroup>
        </FormControl>
      ) : (
        <Typography variant="body1">No options available for this question.</Typography>
      )} 
      {currentQuestionIndex < questions.length - 1 ? (
        <Button onClick={handleNextQuestion} variant="contained" color="primary" disabled={!selectedAnswer}>
          Next Question
        </Button>
      ) : (
        <Button onClick={handleFinishQuiz} variant="contained" color="secondary" disabled={!selectedAnswer}>
          Finish Quiz
        </Button>
      )}
    </Box>
  );
};

export default QuizComponent;
