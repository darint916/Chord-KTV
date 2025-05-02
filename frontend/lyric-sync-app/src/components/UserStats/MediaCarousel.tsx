import React, { useMemo } from 'react';
import {
  Box,
  Card,
  CardActionArea,
  CardMedia,
  CardContent,
  Typography,
} from '@mui/material';
import AlbumIcon from '@mui/icons-material/Album'; // Import for default placeholder

export interface MediaItem {
  id: string;
  title: string;
  subtitle?: string;
  coverUrl: string;
  plays?: number;
  isEmptyState?: boolean;
  dateFavorited?: Date;
  isFavorite?: boolean;
}

interface MediaCarouselProps {
  title: string;
  items: MediaItem[];
  emptyStateMessage?: string; // Optional message for empty state
  onItemClick?: (_item: MediaItem, _index: number) => void;
  fadeColor?: string;
}

const MediaCarousel: React.FC<MediaCarouselProps> = ({
  title,
  items,
  emptyStateMessage = 'Nothing here yet!',
  onItemClick,
  fadeColor,
}) => {
  // Add state and ref for scroll tracking
  const [scrollPercentage, setScrollPercentage] = React.useState(0);
  const scrollContainerRef = React.useRef<HTMLDivElement>(null);

  // Generate skeleton items if the array is empty
  const displayItems = useMemo(() => {
    if (items.length > 0) {
      return items;
    }
    return [{
      id: 'empty-placeholder',
      title: emptyStateMessage,
      coverUrl: '',
      isEmptyState: true, // Custom property to identify skeleton items
      subtitle: undefined,
      plays: undefined,
    }];
  }, [items, emptyStateMessage]);

  // Handle scroll events to calculate the scroll percentage
  const handleScroll = () => {
    const container = scrollContainerRef.current;
    if (container) {
      const scrollLeft = container.scrollLeft;
      const scrollWidth = container.scrollWidth;
      const clientWidth = container.clientWidth;
      const maxScrollLeft = scrollWidth - clientWidth;

      // Calculate scroll percentage (0 to 1)
      const percentage = maxScrollLeft > 0 ? scrollLeft / maxScrollLeft : 0;
      setScrollPercentage(percentage);
    }
  };

  return (
    <Box sx={{ mb: 4 }}>
      <Typography
        variant="h5"
        sx={{ fontWeight: 600, fontFamily: '"Montserrat", sans-serif', mb: 0.0 }}
      >
        {title}
      </Typography>

      {/* wrapper to allow relative positioning for the fade */}
      <Box sx={{ position: 'relative' }}>
        {/* actual scroller */}
        <Box
          ref={scrollContainerRef}
          onScroll={handleScroll}
          sx={{
            display: 'flex',
            overflowX: 'auto',
            overflowY: 'visible',
            gap: 2,
            px: 1,
            py: 2,
            scrollSnapType: 'x mandatory',
            '&::-webkit-scrollbar': { display: 'none' },
          }}
        >
          {displayItems.map((m, index) => (
            <Card
              key={m.id}
              sx={{
                width: 160, // Fixed width to constrain the card even if the title is long.
                flexShrink: 0,
                scrollSnapAlign: 'start',
                borderRadius: 2,
                padding: 1,        // Remove padding
                margin: 0,         // Remove margin
                transition: 'transform .2s, box-shadow .2s',
                ...(m.isEmptyState ? {} : {
                  ':hover': {
                    transform: 'scale(1.05)',
                    boxShadow: '0 0 8px 2px rgba(0,82,204,.4)',
                  }
                }),
                opacity: m.isEmptyState ? 0.7 : 1, // Dimmed appearance for empty state
              }}
              elevation={m.isEmptyState ? 1 : 3}
            >
              <CardActionArea
                sx={{ p: 0 }}
                disabled={m.isEmptyState}
                onClick={() =>
                  !m.isEmptyState && onItemClick?.(m, index)
                }
              >
                {m.coverUrl ? (
                  <CardMedia
                    component="img"
                    height="160"
                    image={m.coverUrl}
                    alt={m.title}
                    sx={{
                      borderTopLeftRadius: 8,
                      borderTopRightRadius: 8,
                      display: 'block',
                      padding: 0,        // Remove padding
                      margin: 0,         // Remove margin
                    }}
                  />
                ) : (
                  // Placeholder for empty state
                  <Box
                    sx={{
                      height: 160,
                      display: 'flex',
                      alignItems: 'center',
                      justifyContent: 'center',
                      bgcolor: 'grey.200',
                      borderTopLeftRadius: 8,
                      borderTopRightRadius: 8,
                    }}
                  >
                    <AlbumIcon sx={{ fontSize: 60, color: 'grey.400' }} />
                  </Box>
                )}
                <CardContent sx={{ py: 1, px: 1.5 }}>
                  <Typography
                    variant="subtitle2"
                    noWrap
                    sx={{
                      fontWeight: 600,
                      color: (m.isEmptyState && index !== 0) ? 'text.secondary' : 'text.primary'
                    }}
                  >
                    {m.title}
                  </Typography>

                  {/* render artist/other subtitle if present */}
                  {m.subtitle && (
                    <Typography
                      variant="caption"
                      color="text.secondary"
                      noWrap
                    >
                      {m.subtitle}
                    </Typography>
                  )}

                  {m.plays !== undefined && (
                    <Typography variant="caption" color="text.secondary">
                      {m.plays} plays
                    </Typography>
                  )}
                </CardContent>
              </CardActionArea>
            </Card>
          ))}
        </Box>

        {/* left‐edge fade overlay (dynamic based on scroll) */}
        <Box
          sx={(theme) => ({
            pointerEvents: 'none',
            position: 'absolute',
            top: 0,
            bottom: 0,
            left: 0,
            width: theme.spacing(30), // 50 * 8px = 400px
            background: `linear-gradient(to left, transparent, ${fadeColor || theme.palette.background.default})`,
            opacity: scrollPercentage, // Increases as we scroll right
          })}
        />

        {/* right‐edge fade overlay (dynamic based on scroll) */}
        <Box
          sx={(theme) => ({
            pointerEvents: 'none',
            position: 'absolute',
            top: 0,
            bottom: 0,
            right: 0,
            width: theme.spacing(30), // 50 * 8px = 400px
            background: `linear-gradient(to right, transparent, ${fadeColor || theme.palette.background.default})`,
            opacity: 1 - scrollPercentage, // Decreases as we scroll right
          })}
        />
      </Box>
    </Box>
  );
};

export default MediaCarousel;
