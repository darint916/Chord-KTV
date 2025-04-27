export interface QueueItem {
  queueId: string;
  title: string;
  artist: string;
  youTubeId: string;
  ktvYouTubeId?: string;
  lyrics: string;
  status: 'pending' | 'loading' | 'loaded';
  imageUrl?: string;
  error?: string;
}