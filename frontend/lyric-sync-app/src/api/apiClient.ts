import { SongApi, Configuration, QuizApi, HandwritingApi, UserApi, UserActivityApi } from './';
import { getAuthToken } from '../utils/auth';

// Use environment variable or fallback to window.location.origin
const basePath = import.meta.env.VITE_API_BASE_URL || window.location.origin;

const apiConfig = new Configuration({
    basePath,
    fetchApi: (input, init = {}) => {
      const token = getAuthToken();
      const headers = new Headers(init.headers);
      if (token) {
        headers.set('Authorization', `Bearer ${token}`);
      }
      return fetch(input, {
        ...init,
        headers,
      });
    },
  });

export const songApi = new SongApi(apiConfig);
export const quizApi = new QuizApi(apiConfig);
export const handwritingApi = new HandwritingApi(apiConfig);
export const userApi = new UserApi(apiConfig);
export const userActivityApi = new UserActivityApi(apiConfig);
