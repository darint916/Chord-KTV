import { FullSongResponseDto } from "../api";

export interface QueueItem {
  queueId: string;
  title: string;
  artist: string;
  youtubeUrl: string;
  lyrics: string;
  apiRequested: boolean;
  processedData?: FullSongResponseDto; // Added when processed
  error?: string;
}