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

  const handleScroll = (e: React.UIEvent<HTMLDivElement>) => {
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
    if (pct >= 100) return theme.palette.success.dark;
    if (pct >= 80) return theme.palette.success.light;
    if (pct >= 60) return theme.palette.warning.main;
    return theme.palette.error.main;
  };

  return (
    <Paper sx={{ position: 'relative', overflow: 'hidden', p: 2 }}>
      <Typography variant="h6" gutterBottom>
        Recent&nbsp;Scores
      </Typography>

      {sortedQuizzes.length === 0 ? (
        <Typography variant="body2" color="text.secondary">
          No quizzes taken yet.
        </Typography>
      ) : (
        <Box sx={{ position: 'relative' }}>
          {/* Scrollable container for all quizzes */}
          <Box
            ref={scrollContainerRef}
            onScroll={handleScroll}
            sx={{
              overflowY: 'auto',
              // Fixed height showing roughly 5 full items (adjust as needed)
              height: 5 * 72, // assuming each quiz item is ~72px tall
              pr: 1,
            }}
          >
            {sortedQuizzes.map((q) => (
              <Box
                key={q.quizId}
                display="flex"
                alignItems="center"
                gap={2}
                mb={2}
              >
                <Chip
                  label={q.language.toUpperCase()}
                  size="small"
                  sx={{ minWidth: 64, ...getLanguageColor(q.language) }}
                />
                <Box sx={{ flexGrow: 1, lineHeight: 0.0 }}>
                  <Typography variant="body2" sx={{ mb: -0.2 }}>
                    {q.dateCompleted
                      ? new Date(q.dateCompleted).toLocaleDateString()
                      : '—'}
                  </Typography>
                  <Typography variant="caption" color="text.disabled">
                    {q.quizType ? q.quizType : 'quiz'}
                  </Typography>
                </Box>
                <Box position="relative" display="inline-flex">
                  <CircularProgress
                    variant="determinate"
                    value={(q.score / 5) * 100}
                    thickness={5}
                    size={40}
                    sx={{ color: getColor(q.score) }}
                  />
                  <Box
                    position="absolute"
                    top={0}
                    left={0}
                    bottom={0}
                    right={0}
                    display="flex"
                    alignItems="center"
                    justifyContent="center"
                  >
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
            sx={(theme) => ({
              pointerEvents: 'none',
              position: 'absolute',
              top: 0,
              left: 0,
              right: 0,
              height: theme.spacing(4), // adjust fade height as needed
              background: `linear-gradient(to bottom, white, transparent)`,
              opacity: scrollPercentage,
            })}
          />

          {/* Bottom fade overlay (visible initially when scrolled to top) */}
          <Box
            sx={(theme) => ({
              pointerEvents: 'none',
              position: 'absolute',
              bottom: 0,
              left: 0,
              right: 0,
              height: theme.spacing(4), // adjust fade height as needed
              background: `linear-gradient(to top, white, transparent)`,
              opacity: 1 - scrollPercentage,
            })}
          />
        </Box>
      )}
    </Paper>
  );
};

export default QuizResultsSection;
