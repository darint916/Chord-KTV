import React from 'react';
import {
  Box,
  Chip,
  CircularProgress,
  Paper,
  Typography,
  useTheme,
} from '@mui/material';
import { getLanguageColor } from '../../utils/languageColorUtils';
import styles from './QuizResultsSection.module.scss';

interface QuizDto {
  quizId: string;
  score: number; // out of 5
  language: string;
  dateCompleted?: string | null;
  quizType?: string; // field to indicate the quiz type (romanization, handwriting, etc.)
}

interface QuizResultsSectionProps {
  quizzes: QuizDto[];
}

const QuizResultsSection: React.FC<QuizResultsSectionProps> = ({ quizzes }) => {
  const theme = useTheme();
  const [scrollPercentage, setScrollPercentage] = React.useState(0);
  const scrollContainerRef = React.useRef<HTMLDivElement>(null);

  // Sort quizzes by dateCompleted, most recent first
  const sortedQuizzes = React.useMemo(
    () =>
      [...quizzes].sort((a, b) => {
        return (
          new Date(b.dateCompleted ?? 0).getTime() -
          new Date(a.dateCompleted ?? 0).getTime()
        );
      }),
    [quizzes]
  );

  const handleScroll = () => {
    const container = scrollContainerRef.current;
    if (container) {
      const scrollTop = container.scrollTop;
      const scrollHeight = container.scrollHeight;
      const clientHeight = container.clientHeight;
      const maxScrollTop = scrollHeight - clientHeight;
      const percentage = maxScrollTop > 0 ? scrollTop / maxScrollTop : 0;
      setScrollPercentage(percentage);
    }
  };

  const getColor = (score: number) => {
    const pct = (score / 5) * 100;
    if (pct >= 100) {return theme.palette.success.dark;}
    if (pct >= 80) {return theme.palette.success.light;}
    if (pct >= 60) {return theme.palette.warning.main;}
    return theme.palette.error.main;
  };

  return (
    <Paper className={styles.quizResultsPaper}>
      <Typography variant="h6" gutterBottom className={styles.sectionTitle}>
        Recent&nbsp;Scores
      </Typography>
      <Box component="hr" className={styles.hr} />
      {sortedQuizzes.length === 0 ? (
        <Typography variant="body2" color="text.secondary">
          No quizzes taken yet.
        </Typography>
      ) : (
        <Box style={{ position: 'relative' }}>
          {/* Scrollable container for all quizzes */}
          <Box
            ref={scrollContainerRef}
            onScroll={handleScroll}
            className={styles.scrollContainer}
          >
            {sortedQuizzes.map((q) => (
              <Box
                key={q.quizId}
                className={styles.quizItem}
              >
                <Chip
                  label={q.language.toUpperCase()}
                  size="small"
                  sx={{ minWidth: 64, ...getLanguageColor(q.language) }}
                />
                <Box className={styles.quizInfo}>
                  <Typography variant="body2" className={styles.quizDate}>
                    {q.dateCompleted
                      ? new Date(q.dateCompleted).toLocaleDateString()
                      : '—'}
                  </Typography>
                  <Typography variant="caption" className={styles.quizType}>
                    {q.quizType ? q.quizType : 'quiz'}
                  </Typography>
                </Box>
                <Box className={styles.progressCircle}>
                  <CircularProgress
                    variant="determinate"
                    value={(q.score / 5) * 100}
                    thickness={5}
                    size={40}
                    sx={{ color: getColor(q.score) }}
                  />
                  <Box className={styles.progressLabel}>
                    <Typography variant="caption" component="div">
                      {q.score}/5
                    </Typography>
                  </Box>
                </Box>
              </Box>
            ))}
          </Box>

          {/* Top fade overlay (appears as you scroll down) */}
          <Box
            className={styles.fadeTop}
            sx={{ opacity: scrollPercentage }}
          />

          {/* Bottom fade overlay (visible initially when scrolled to top) */}
          <Box
            className={styles.fadeBottom}
            sx={{ opacity: 1 - scrollPercentage }}
          />
        </Box>
      )}
    </Paper>
  );
};

export default QuizResultsSection;
