import { FullSongResponseDto } from "../api";

export interface QueueItem {
  queueId: string;
  title: string;
  artist: string;
  youtubeUrl: string;
  lyrics: string;
  processedData?: FullSongResponseDto; // Added when processed
  processing?: boolean;
  error?: string;
}