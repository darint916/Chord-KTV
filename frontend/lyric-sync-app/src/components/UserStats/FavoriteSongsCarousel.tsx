import React from 'react';
import MediaCarousel, { MediaItem } from './MediaCarousel';

interface FavoriteSongsCarouselProps {
  songs: MediaItem[];
}

const FavoriteSongsCarousel: React.FC<FavoriteSongsCarouselProps> = ({
  songs,
}) => <MediaCarousel title="Favorite Songs" items={songs} />;

export default FavoriteSongsCarousel;
