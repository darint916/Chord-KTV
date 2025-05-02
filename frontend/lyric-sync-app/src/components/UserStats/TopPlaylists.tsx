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
  IconButton,
} from '@mui/material';
import { MediaItem } from './MediaCarousel';
import styles from './TopPlaylists.module.scss';
import FavoriteIcon from '@mui/icons-material/Favorite';
import FavoriteBorderIcon from '@mui/icons-material/FavoriteBorder';
interface TopPlaylistsProps {
  data: MediaItem[];
  onToggleFavorite: (_playlistId: string, _isFav: boolean) => void;
}

const TopPlaylists: React.FC<TopPlaylistsProps> = ({ data, onToggleFavorite}) => {
  const [scrollPct, setScrollPct] = React.useState(0);
  const ref = React.useRef<HTMLDivElement>(null);

  const handleScroll = () => {
    if (!ref.current) { return; }
    const { scrollTop, scrollHeight, clientHeight } = ref.current;
    const max = scrollHeight - clientHeight;
    setScrollPct(max > 0 ? scrollTop / max : 0);
  };

  const [isFavMap, setIsFavMap] = React.useState<Record<string, boolean>>({});
  React.useEffect(() => {
    const map = data.reduce((acc, p) => {
      acc[p.id] = p.isFavorite ?? false;
      return acc;
    }, {} as Record<string, boolean>);
    setIsFavMap(map);
  }, [data]);
  const handleToggle = (id: string) => {
    const newVal = !isFavMap[id];
    setIsFavMap(m => ({ ...m, [id]: newVal }));
    onToggleFavorite(id, newVal);
  };


  return (
    <Paper elevation={0} className={styles.topPlaylistsPaper}>
      <Typography variant="h6" gutterBottom>
        Top Playlists
      </Typography>
      <Box component="hr" className={styles.divider} />

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
                <ListItem key={p.id}
                  secondaryAction={
                    <IconButton edge="end" size='small' onClick={() => handleToggle(p.id)}>
                      {isFavMap[p.id] ? <FavoriteIcon color="error" fontSize="small" /> : <FavoriteBorderIcon fontSize="small"/>}
                    </IconButton>
                  }>
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
