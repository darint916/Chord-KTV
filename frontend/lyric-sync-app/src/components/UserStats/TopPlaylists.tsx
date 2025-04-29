import React from 'react';
import {
  Box,
  Paper,
  List,
  ListItem,
  ListItemAvatar,
  Avatar,
  ListItemText,
  Typography,
} from '@mui/material';
import { MediaItem } from './MediaCarousel';
import styles from './TopPlaylists.module.scss';

interface TopPlaylistsProps {
  data: MediaItem[];
}

const TopPlaylists: React.FC<TopPlaylistsProps> = ({ data }) => {
  const [scrollPct, setScrollPct] = React.useState(0);
  const ref = React.useRef<HTMLDivElement>(null);

  const handleScroll = () => {
    if (!ref.current) {return;}
    const { scrollTop, scrollHeight, clientHeight } = ref.current;
    const max = scrollHeight - clientHeight;
    setScrollPct(max > 0 ? scrollTop / max : 0);
  };

  return (
    <Paper elevation={0}>
      <Typography variant="h5" gutterBottom>
        Top Playlists
      </Typography>

      {data.length === 0 ? (
        <Typography variant="body2" color="text.secondary">
          No playlist activity yet.
        </Typography>
      ) : (
        <Box style={{ position: 'relative' }}>
          <Box
            ref={ref}
            onScroll={handleScroll}
            className={styles.scrollContainer}
          >
            <List dense disablePadding>
              {data.map((p) => (
                <ListItem key={p.id}>
                  <ListItemAvatar>
                    <Avatar
                      variant="square"
                      src={p.coverUrl}
                      sx={{ width: 48, height: 48, mr: 1 }}
                    />
                  </ListItemAvatar>
                  <ListItemText
                    primary={p.title}
                    secondary={`${p.plays} plays`}
                    secondaryTypographyProps={{ color: 'text.secondary' }}
                  />
                </ListItem>
              ))}
            </List>
          </Box>

          {/* fade overlays (optional) */}
          <Box
            className={styles.fadeTop}
            style={{ opacity: scrollPct }}
          />
          <Box
            className={styles.fadeBottom}
            style={{ opacity: 1 - scrollPct }}
          />
        </Box>
      )}
    </Paper>
  );
};

export default TopPlaylists;
