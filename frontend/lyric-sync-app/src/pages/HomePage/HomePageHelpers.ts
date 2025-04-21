export const extractYouTubeVideoId = (url: string | null | undefined): string | null => {
  if (!url) { return null; }
  const match = url.match(/(?:\?v=|\/embed\/|\.be\/|\/watch\?v=|\/watch\?.+&v=)([a-zA-Z0-9_-]{11})/);
  return match ? match[1] : null;
};

export const extractPlaylistId = (url: string): string | null => {
  if (!url) { return null; }
  const match = url.match(/[?&]list=([a-zA-Z0-9_-]+)/);
  return match ? match[1] : null;
};