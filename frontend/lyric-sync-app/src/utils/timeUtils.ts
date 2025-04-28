/**
 * Converts .NET TimeSpan string ("hh:mm:ss" or "mm:ss" etc) into seconds.
 */
export function parseTimeSpan(timeSpan?: string | null): number {
  if (!timeSpan) {return 0;}
  const parts = timeSpan.split(':').map(p => parseFloat(p));
  if (parts.length === 3) {
    const [h, m, s] = parts;
    return h * 3600 + m * 60 + s;
  }
  if (parts.length === 2) {
    const [m, s] = parts;
    return m * 60 + s;
  }
  return parseFloat(timeSpan) || 0;
} 