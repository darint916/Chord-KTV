import React from 'react';
import {
  Avatar,
  Box,
  Paper,
  Typography,
  LinearProgress,
} from '@mui/material';
import MusicNoteIcon from '@mui/icons-material/MusicNote';
import styles from './TopSongsChart.module.scss';

interface SongDatum {
  id: string;
  title: string;
  plays: number;
}

interface TopSongsChartProps {
  data: SongDatum[];
}

const TopSongsChart: React.FC<TopSongsChartProps> = ({ data }) => {
  const maxPlays = Math.max(1, ...data.map((d) => d.plays));

  // Manage scroll state for fade overlays
  const [scrollPercentage, setScrollPercentage] = React.useState(0);
  const scrollContainerRef = React.useRef<HTMLDivElement>(null);

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

  return (
    <Paper className={styles.topSongsPaper}>
      <Typography variant="h6" gutterBottom>
        Top Songs
      </Typography>
      <Box component="hr" className={styles.divider} />
      

      {data.length === 0 ? (
        <Typography variant="body2" color="text.secondary">
          No song activity yet.
        </Typography>
      ) : (
        <Box style={{ position: 'relative' }}>
          {/* Scrollable container for the top songs */}
          <Box
            ref={scrollContainerRef}
            onScroll={handleScroll}
            className={styles.scrollContainer}
          >
            {data.map((s) => (
              <Box key={s.id} className={styles.songBox}>
                <Box className={styles.songRow}>
                  <Avatar variant="rounded">
                    <MusicNoteIcon fontSize="small" />
                  </Avatar>
                  <Typography variant="subtitle2" className={styles.songTitle}>
                    {s.title ?? s.id}
                  </Typography>
                  <Typography
                    variant="caption"
                    className={styles.songPlays}
                  >
                    {s.plays} plays
                  </Typography>
                </Box>
                <LinearProgress
                  variant="determinate"
                  value={(s.plays / maxPlays) * 100}
                  className={styles.progressBar}
                />
              </Box>
            ))}
          </Box>

          {/* Top fade overlay (appears as you scroll down) */}
          <Box
            className={styles.fadeTop}
            style={{ opacity: scrollPercentage }}
          />

          {/* Bottom fade overlay (visible initially when at the top) */}
          <Box
            className={styles.fadeBottom}
            style={{ opacity: 1 - scrollPercentage }}
          />
        </Box>
      )}
    </Paper>
  );
};

export default TopSongsChart;
