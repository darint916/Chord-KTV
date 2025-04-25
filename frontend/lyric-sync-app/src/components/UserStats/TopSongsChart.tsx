import React from 'react';
import {
  Avatar,
  Box,
  Paper,
  Typography,
  LinearProgress,
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
  const maxPlays = Math.max(1, ...data.map((d) => d.plays));

  return (
    <Paper>
      <Typography variant="h6" gutterBottom>
        Top&nbsp;Songs
      </Typography>

      {data.length === 0 ? (
        <Typography variant="body2" color="text.secondary">
          No song activity yet.
        </Typography>
      ) : (
        data.map((s) => (
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
        ))
      )}
    </Paper>
  );
};

export default TopSongsChart;
