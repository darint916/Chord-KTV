import React from 'react';
import MediaCarousel, { MediaItem } from './MediaCarousel';

interface FavoritePlaylistsCarouselProps {
  playlists: MediaItem[];
}

const FavoritePlaylistsCarousel: React.FC<FavoritePlaylistsCarouselProps> = ({
  playlists,
}) => <MediaCarousel title="Favorite Playlists" items={playlists} />;

export default FavoritePlaylistsCarousel;
