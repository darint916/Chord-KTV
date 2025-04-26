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
} from '@mui/material';
import QueueMusicIcon from '@mui/icons-material/QueueMusic';
import styles from './TopPlaylists.module.scss';

interface PlaylistDatum {
  id: string;
  plays: number;
}

interface TopPlaylistsProps {
  data: PlaylistDatum[];
}

const TopPlaylists: React.FC<TopPlaylistsProps> = ({ data }) => {
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
    <Paper className={styles.topPlaylistsPaper}>
      <Box className={styles.headerRow}>
        <Typography variant="h6">Top&nbsp;Playlists</Typography>
      </Box>
      <Box component="hr" className={styles.divider} />
      
      {data.length === 0 ? (
        <Typography variant="body2" color="text.secondary">
          No playlist activity yet.
        </Typography>
      ) : (
        <Box style={{ position: 'relative' }}>
          {/* Scrollable container with exact height */}
          <Box
            ref={scrollContainerRef}
            onScroll={handleScroll}
            className={styles.scrollContainer}
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
            className={styles.fadeTop}
            style={{ opacity: scrollPercentage }}
          />

          {/* Bottom fade overlay */}
          <Box
            className={styles.fadeBottom}
            style={{ opacity: 1 - scrollPercentage }}
          />
        </Box>
      )}
    </Paper>
  );
};

export default TopPlaylists;
