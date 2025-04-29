/**
 * Aggregates an array of items by a key and metric.
 * @param arr The source array
 * @param keyExtractor A function to extract a grouping key from an item.
 * @param metricExtractor A function to extract the numeric metric from an item.
 * @param top How many items to return.
 */
export const getTopAggregatedItems = <T>(
  arr: T[],
  keyExtractor: (_item: T) => string,
  metricExtractor: (_item: T) => number,
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
      quizType: q.quizType || 'romanize/audio',
    }))
    .concat(
      handwriting.map((q) => ({
        ...(q as unknown as Q),
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

export const safeFetch = async <T>(
  promise: Promise<T>,
  logout: () => void
): Promise<T> => {
  try {
    return await promise;
  } catch (error: unknown) {
    // Narrow to the minimal shape you expect
    const err = error as { response?: { status: number }; status?: number };

    if (err.response?.status === 401 || err.status === 401) {
      logout();
    }

    // Any other error → empty value
    return [] as unknown as T;
  }
};
