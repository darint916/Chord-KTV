import React from 'react';
import {
  Avatar,
  Box,
  Paper,
  Typography,
  LinearProgress,
  useTheme,
} from '@mui/material';
import MusicNoteIcon from '@mui/icons-material/MusicNote';

interface SongDatum {
  id: string;
  title: string;
  plays: number;
}

interface TopSongsChartProps {
  data: SongDatum[];
}

const TopSongsChart: React.FC<TopSongsChartProps> = ({ data }) => {
  const theme = useTheme();
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
    <Paper sx={{ position: 'relative', p: 3, display: 'flex', flexDirection: 'column', flex: 1 }}>
      <Typography variant="h6" gutterBottom>
        Top Songs
      </Typography>
      <Box component="hr" sx={{ border: 0, borderTop: '1px solid', borderColor: 'grey.300', width: '100%', my: 1 }} />
      

      {data.length === 0 ? (
        <Typography variant="body2" color="text.secondary">
          No song activity yet.
        </Typography>
      ) : (
        <Box sx={{ position: 'relative' }}>
          {/* Scrollable container for the top songs */}
          <Box
            ref={scrollContainerRef}
            onScroll={handleScroll}
            sx={{ overflowY: 'auto', flex: 1, pr: 1 }}
          >
            {data.map((s) => (
              <Box key={s.id} mb={2}>
                <Box display="flex" alignItems="center" gap={2} mb={1}>
                  <Avatar variant="rounded">
                    <MusicNoteIcon fontSize="small" />
                  </Avatar>
                  <Typography variant="subtitle2" noWrap>
                    {s.title ?? s.id}
                  </Typography>
                  <Typography
                    variant="caption"
                    sx={{ ml: 'auto', color: 'text.secondary' }}
                  >
                    {s.plays} plays
                  </Typography>
                </Box>
                <LinearProgress
                  variant="determinate"
                  value={(s.plays / maxPlays) * 100}
                  sx={{ height: 10, borderRadius: 5 }}
                />
              </Box>
            ))}
          </Box>

          {/* Top fade overlay (appears as you scroll down) */}
          <Box
            sx={{
              pointerEvents: 'none',
              position: 'absolute',
              top: 0,
              left: 0,
              right: 0,
              height: theme.spacing(4),
              background: 'linear-gradient(to bottom, white, transparent)',
              opacity: scrollPercentage,
            }}
          />

          {/* Bottom fade overlay (visible initially when at the top) */}
          <Box
            sx={{
              pointerEvents: 'none',
              position: 'absolute',
              bottom: 0,
              left: 0,
              right: 0,
              height: theme.spacing(4),
              background: 'linear-gradient(to top, white, transparent)',
              opacity: 1 - scrollPercentage,
            }}
          />
        </Box>
      )}
    </Paper>
  );
};

export default TopSongsChart;
