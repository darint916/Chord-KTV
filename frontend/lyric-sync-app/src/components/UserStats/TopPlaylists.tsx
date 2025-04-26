import React from 'react';
import {
  Avatar,
  Box,
  List,
  ListItem,
  ListItemAvatar,
  ListItemText,
  Paper,
  Typography,
  useTheme,
} from '@mui/material';
import QueueMusicIcon from '@mui/icons-material/QueueMusic';

interface PlaylistDatum {
  id: string;
  plays: number;
}

interface TopPlaylistsProps {
  data: PlaylistDatum[];
}

const TopPlaylists: React.FC<TopPlaylistsProps> = ({ data }) => {
  const theme = useTheme();
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
    <Paper
      sx={{
        position: 'relative',
        p: 3,
        display: 'flex',
        flexDirection: 'column',
        flex: 1,
        minWidth: 260,
      }}
    >
      <Box display="flex" justifyContent="space-between" alignItems="center" mb={1}>
        <Typography variant="h6">Top&nbsp;Playlists</Typography>
      </Box>
      <Box component="hr" sx={{ border: 0, borderTop: '1px solid', borderColor: 'grey.300', width: '100%', my: 1 }} />
      
      {data.length === 0 ? (
        <Typography variant="body2" color="text.secondary">
          No playlist activity yet.
        </Typography>
      ) : (
        <Box sx={{ position: 'relative' }}>
          {/* Scrollable container with exact height */}
          <Box
            ref={scrollContainerRef}
            onScroll={handleScroll}
            sx={{ overflowY: 'auto', flex: 1, pr: 1 }}
          >
            <List dense disablePadding>
              {data.map((p) => (
                <ListItem key={p.id}>
                  <ListItemAvatar>
                    <Avatar variant="rounded" sx={{ bgcolor: 'secondary.main' }}>
                      <QueueMusicIcon />
                    </Avatar>
                  </ListItemAvatar>
                  <ListItemText
                    primary={p.id}
                    secondary={`${p.plays} plays`}
                    secondaryTypographyProps={{ color: 'text.secondary' }}
                  />
                </ListItem>
              ))}
            </List>
          </Box>

          {/* Top fade overlay */}
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

          {/* Bottom fade overlay */}
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

export default TopPlaylists;
