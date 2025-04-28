import React from 'react';
import { Box } from '@mui/material';
import MediaCarousel, { MediaItem } from '../UserStats/MediaCarousel';
import type { GeniusHitDto } from '../../api/models/GeniusHitDto';

export interface GeniusHitsCarouselProps {
  /** raw hits from Genius search */
  hits: GeniusHitDto[];
  /** callback when user picks one hit */
  onSelect: (hit: GeniusHitDto) => void;
  /** optional overrides */
  title?: string;
  fadeColor?: string;
}

const DEFAULT_TITLE = 'Select a Song';
const DEFAULT_FADE = '#E0E7FF';

const GeniusHitsCarousel: React.FC<GeniusHitsCarouselProps> = ({
  hits,
  onSelect,
  title = DEFAULT_TITLE,
  fadeColor = DEFAULT_FADE,
}) => {
  // map raw hits → MediaItem
  const items: MediaItem[] = React.useMemo(
    () =>
      hits.map((h) => {
        // some DTOs use camelCase, some snake_case
        const coverUrl =
          h.result.songArtImageUrl ||
          h.result.headerImageUrl ||
          // fallback names
          (h.result as any).song_art_image_url ||
          (h.result as any).header_image_url ||
          '';
        const artistNames =
          (h.result as any).primaryArtistNames ||
          (h.result as any).primary_artist_names ||
          '';
        return {
          id: String(h.result.id),
          /** only the song title here */
          title: h.result.title ?? '',
          /** separate artist line */
          subtitle: artistNames,
          coverUrl,
        };
      }),
    [hits]
  );

  if (items.length === 0) return null;

  const handleItemClick = (_item: MediaItem, index: number) => {
    onSelect(hits[index]);
  };

  return (
    <Box mt={4}>
      <MediaCarousel
        title={title}
        items={items}
        onItemClick={handleItemClick}
        fadeColor={fadeColor}
      />
    </Box>
  );
};

export default GeniusHitsCarousel; 