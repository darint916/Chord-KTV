import { SongApi, Configuration } from './';


const apiConfig = new Configuration({
  basePath: import.meta.env.VITE_API_URL || 'http://localhost:5259',
});

export const songApi = new SongApi(apiConfig);

