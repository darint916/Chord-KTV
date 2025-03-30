import { SongApi, Configuration, QuizApi, HandwritingApi } from './';


const apiConfig = new Configuration({
  basePath: import.meta.env.VITE_API_URL || 'http://localhost:5259',
});

export const songApi = new SongApi(apiConfig);
export const quizApi = new QuizApi(apiConfig);
export const handwritingApi = new HandwritingApi(apiConfig);
