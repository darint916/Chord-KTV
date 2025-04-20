export const extractYouTubeVideoId = (url: string | null | undefined): string | null => {
  if (!url) { return null; }
  const match = url.match(/(?:\?v=|\/embed\/|\.be\/|\/watch\?v=|\/watch\?.+&v=)([a-zA-Z0-9_-]{11})/);
  return match ? match[1] : null;
};