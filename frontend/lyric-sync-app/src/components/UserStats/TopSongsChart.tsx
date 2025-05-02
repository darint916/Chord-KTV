import React, { useState } from 'react';
import {
  Avatar,
  Box,
  Paper,
  Typography,
  LinearProgress,
  IconButton,
} from '@mui/material';
import MusicNoteIcon from '@mui/icons-material/MusicNote';
import styles from './TopSongsChart.module.scss';
import { MediaItem } from './MediaCarousel';
import { userActivityApi } from '../../api/apiClient';
import FavoriteBorderIcon from '@mui/icons-material/FavoriteBorder';
import FavoriteIcon from '@mui/icons-material/Favorite';

// interface SongDatum {
//   id: string;
//   title: string;
//   plays: number;
// }

interface TopSongsChartProps {
  // data: SongDatum[];
  mediaItems: MediaItem[];
}

const TopSongsChart: React.FC<TopSongsChartProps> = ({ mediaItems }) => {
  const maxPlays = Math.max(1, ...mediaItems.map((d) => d.plays).filter((plays): plays is number => plays !== undefined));

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

  const [favLoadingMap, setFavLoadingMap] = useState<Record<string, boolean>>({});
  const [isFavMap, setIsFavMap] = useState<Record<string, boolean>>(
    () => mediaItems.reduce((acc, s) => {
      acc[s.id] = s.isFavorite ?? false;
      return acc;
    }, {} as Record<string, boolean>)
  );

  // Toggle favorite & call the PATCH endpoint
  const handleToggleFavorite = async (_songId: string) => {
    if (!_songId) { return; }
    setFavLoadingMap((m: Record<string, boolean>) => ({ ...m, [_songId]: true }));
    try {
      await userActivityApi.apiUserActivityFavoriteSongPatch({
        userSongActivityFavoriteRequestDto: {
          songId: _songId,
          isFavorite: !isFavMap[_songId],
        }
      });
      setIsFavMap((m: Record<string, boolean>) => ({ ...m, [_songId]: !m[_songId] }));
    } catch {
      // console.error('Failed to toggle favorite', error);
    } finally {
      setFavLoadingMap((m: Record<string, boolean>) => ({ ...m, [_songId]: false }));
    }
  };


  return (
    <Paper className={styles.topSongsPaper}>
      <Typography variant="h6" gutterBottom>
        Top Songs
      </Typography>
      <Box component="hr" className={styles.divider} />


      {mediaItems.length === 0 ? (
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
            {mediaItems.map((s) => (
              <Box key={s.id} className={styles.songBox}>
                <Box className={styles.songRow}>
                  <Avatar variant="rounded"
                    src={s.coverUrl}>
                    <MusicNoteIcon fontSize="small" />
                  </Avatar>
                  <Typography variant="subtitle2" className={styles.songTitle}>
                    {s.title ?? s.id}
                  </Typography>
                  <Typography
                    variant="caption"
                    className={styles.songPlays}
                  >
                    {s.plays} {s.plays === 1 ? 'play' : 'plays'}
                  </Typography>
                  <IconButton
                    className={styles.favoriteButton}
                    size="small"
                    onClick={() => handleToggleFavorite(s.id)}
                    disabled={favLoadingMap[s.id]}
                    sx={{ ml: 'auto' }}
                  >
                    {isFavMap[s.id]
                      ? <FavoriteIcon color="error" fontSize="small" />
                      : <FavoriteBorderIcon fontSize="small" />
                    }
                  </IconButton>
                </Box>
                <LinearProgress
                  variant="determinate"
                  value={((s.plays ?? 0) / maxPlays) * 100}
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
