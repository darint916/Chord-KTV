/**
 * Aggregates an array of items by a key and metric.
 * @param arr The source array
 * @param keyExtractor A function to extract a grouping key from an item.
 * @param metricExtractor A function to extract the numeric metric from an item.
 * @param top How many items to return.
 */
export const getTopAggregatedItems = <T>(
  arr: T[],
  keyExtractor: (item: T) => string,
  metricExtractor: (item: T) => number,
  top: number = 3
): { id: string; plays: number }[] => {
  // Group and aggregate metric by key
  const counts = arr.reduce<Record<string, number>>((acc, item) => {
    const key = keyExtractor(item);
    acc[key] = (acc[key] ?? 0) + metricExtractor(item);
    return acc;
  }, {});

  // Sort and pick the top items
  return Object.entries(counts)
    .sort((a, b) => b[1] - a[1])
    .slice(0, top)
    .map(([id, plays]) => ({ id, plays }));
};

/**
 * Merges quiz and handwriting results into a single array for display
 * @param quizzes Array of quiz results
 * @param handwriting Array of handwriting results
 * @param limit Maximum number of results to return (default: 10)
 */
export const getMergedQuizResults = <
  Q extends { quizType?: string },
  H extends { quizType?: string }
>(
  quizzes: Q[],
  handwriting: H[],
  limit: number = 10
) => {
  return quizzes
    .map((q) => ({
      ...q,
      quizType: q.quizType || 'romanization',
    }))
    .concat(
      handwriting.map((q) => ({
        ...(q as Q),
        quizType: 'handwriting',
      }))
    )
    .slice(0, limit);
};

/**
 * Safely fetches data from API and handles authentication errors.
 * @param promise The promise representing the API call.
 * @param logout Function to call if the user needs to be logged out due to authentication issues.
 * @returns The payload from the API response, or an empty array if an error occurs.
 */

export const safeFetch = async <T>(promise: Promise<T>, logout: () => void): Promise<T> => {
  try {
    const res: any = await promise;        // whatever the client returned

    /* ---------- unwrap / normalise ---------- */
    const status: number =
      res && typeof res === 'object' && 'status' in res ? res.status : 200;

    // Some clients wrap the body in `data`, others return the body directly.
    const payload: any =
      res && typeof res === 'object' && 'data' in res ? res.data : res;

    /* ---------- special cases ---------- */
    if (status === 401) {
      console.warn('Token expired – logging out');
      logout();
      return [] as unknown as T;            // keep the component code happy
    }

    if (status === 204 || payload == null) {
      // "No Content" – treat as an empty collection
      return [] as unknown as T;
    }

    /* ---------- happy path ---------- */
    return payload as T;
  } catch (err: any) {
    /* network / parse errors land here */
    if (err?.response?.status === 401 || err?.status === 401) {
      console.warn('Token expired (thrown path) – logging out');
      logout();
    }

    // Anything else → return the neutral "empty" value
    return [] as unknown as T;
  }
};