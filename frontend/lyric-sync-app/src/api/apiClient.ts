import { SongApi, Configuration, QuizApi, HandwritingApi, UserApi, UserActivityApi } from './';
import {
  getAuthToken, setAuthToken, clearAuthToken,
  getRefreshToken, setRefreshToken, clearRefreshToken,
} from './authStorage';

// Use environment variable or fallback to window.location.origin
const basePath = import.meta.env.VITE_API_BASE_URL || window.location.origin;

let refreshPromise: Promise<string | null> | null = null;

async function doRefresh(): Promise<string | null> {
  const refreshToken = getRefreshToken();
  if (!refreshToken) {return null;}

  try {
    const res = await fetch(`${basePath}/api/auth/refresh`, {
      method: 'POST',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify({ refreshToken }),
    });

    if (!res.ok) {
      clearAuthToken(); clearRefreshToken();
      return null;
    }

    const { accessToken: newJwt, refreshToken: newRt } = await res.json();
    setAuthToken(newJwt);
    setRefreshToken(newRt);
    return newJwt;
  } catch {
    clearAuthToken(); clearRefreshToken();
    return null;
  }
}

const fetchWithAutoRefresh: typeof fetch = async (input, init = {}) => {
  // Attach the current JWT
  const headers = new Headers((init as RequestInit).headers || {});
  const jwt = getAuthToken();
  if (jwt) {headers.set('Authorization', `Bearer ${jwt}`);}

  const res = await fetch(input, { ...init, headers });
  if (res.status !== 401) {return res;}

  // Got 401, make (or wait for) a single refresh call
  if (!refreshPromise) {
    refreshPromise = doRefresh().finally(() => (refreshPromise = null));
  }

  const newJwt = await refreshPromise;
  if (!newJwt) {return res;}

  // Retry original request with fresh JWT
  const retryHeaders = new Headers(headers);
  retryHeaders.set('Authorization', `Bearer ${newJwt}`);
  return fetch(input, { ...init, headers: retryHeaders });
};

const apiConfig = new Configuration({
  basePath,
  fetchApi: fetchWithAutoRefresh,
});

export const songApi = new SongApi(apiConfig);
export const quizApi = new QuizApi(apiConfig);
export const handwritingApi = new HandwritingApi(apiConfig);
export const userApi = new UserApi(apiConfig);
export const userActivityApi = new UserActivityApi(apiConfig);

export default fetchWithAutoRefresh;
