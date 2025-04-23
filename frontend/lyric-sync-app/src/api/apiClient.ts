import { SongApi, Configuration, QuizApi, HandwritingApi } from './';

// Use environment variable or fallback to window.location.origin
const basePath = import.meta.env.VITE_API_BASE_URL || window.location.origin;
const apiConfig = new Configuration({ basePath });

export const songApi = new SongApi(apiConfig);
export const quizApi = new QuizApi(apiConfig);
export const handwritingApi = new HandwritingApi(apiConfig);
