import React, { useEffect, useMemo, useState } from 'react';
import { Alert, Box, CircularProgress, Container, ThemeProvider, Typography, } from '@mui/material';
import Grid from '@mui/material/Grid2';
import { useAuth } from '../../contexts/AuthTypes';
import { userActivityApi } from '../../api/apiClient';
import FavoriteSongsCarousel from '../../components/UserStats/FavoriteSongsCarousel';
import FavoritePlaylistsCarousel from '../../components/UserStats/FavoritePlaylistsCarousel';
import TopSongsChart from '../../components/UserStats/TopSongsChart';
import TopPlaylists from '../../components/UserStats/TopPlaylists';
import QuizResultsSection from '../../components/UserStats/QuizResultsSection';
import LearnedWords from '../../components/UserStats/LearnedWords';
import dashboardTheme from '../../theme/dashboardTheme';
import { MediaItem } from '../../components/UserStats/MediaCarousel';
import { getTopAggregatedItems, safeFetch, getMergedQuizResults } from './statsHelpers';
import styles from './UserStatsPage.module.scss';
import KpiStrip from '../../components/UserStats/KPIStrip';
import { UserSongActivityDto, UserPlaylistActivityDto, UserQuizResultDto, UserHandwritingResultDto, LearnedWordDto } from '../../api';

const UserStatsPage: React.FC = () => {
  const { user, logout } = useAuth();

  /* state */
  const [songs, setSongs] = useState<UserSongActivityDto[]>([]);
  const [playlists, setPlaylists] = useState<UserPlaylistActivityDto[]>([]);
  const [favoriteSongs, setFavoriteSongs] = useState<UserSongActivityDto[]>([]);
  const [favoritePlaylists, setFavoritePlaylists] = useState<UserPlaylistActivityDto[]>([]);
  const [quizzes, setQuizzes] = useState<UserQuizResultDto[]>([]);
  const [handwriting, setHandwriting] = useState<UserHandwritingResultDto[]>([]);
  const [words, setWords] = useState<LearnedWordDto[]>([]);
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);

  /* data fetch */
  useEffect(() => {
    if (!user) { return; }
    const ctrl = new AbortController();
    (async () => {
      try {
        setLoading(true);
        const [s, p, favS, favP, q, w, h] = await Promise.all([
          safeFetch(userActivityApi.apiUserActivitySongsGet({ signal: ctrl.signal }), logout),
          safeFetch(userActivityApi.apiUserActivityPlaylistsGet({ signal: ctrl.signal }), logout),
          safeFetch(userActivityApi.apiUserActivityFavoriteSongsGet({ signal: ctrl.signal }), logout),
          safeFetch(userActivityApi.apiUserActivityFavoritePlaylistsGet({ signal: ctrl.signal }), logout),
          safeFetch(userActivityApi.apiUserActivityQuizzesGet(), logout),
          safeFetch(userActivityApi.apiUserActivityLearnedWordsGet(), logout),
          safeFetch(userActivityApi.apiUserActivityHandwritingGet(), logout),
        ]);
        setSongs(s);
        setPlaylists(p);
        setFavoriteSongs(favS);
        setFavoritePlaylists(favP);
        setQuizzes(q.map(item => ({
          ...item,
          quizId: item.quizId ?? 'unknown-quiz', // Ensure quizId is always a string
          score: item.score ?? 0, // Ensure score is always a number
          language: item.language ?? undefined, // Ensure language is always a string
          dateCompleted: item.dateCompleted ? new Date(item.dateCompleted) : null, // Convert Date to string
        })));
        setWords(w.map(item => ({
          ...item,
          word: item.word ?? '',
          language: item.language ?? undefined,
          dateLearned: item.dateLearned ? new Date(item.dateLearned) : null
        })));
        setHandwriting(h);
      } catch (e) {
        const msg = e instanceof Error ? e.message : 'Unknown error';
        setError(msg);
      } finally {
        setLoading(false);
      }
    })();
    return () => ctrl.abort();
  }, [user]);

  /* KPI numbers */
  const kpis = useMemo(() => {
    const totalPlays = songs.reduce((acc, s) => acc + (s.datesPlayed?.length ?? 0), 0);
    return [
      { label: 'Unique Songs', value: new Set(songs.map((s) => s.songId)).size },
      { label: 'Song Plays', value: totalPlays },
      { label: 'Playlists', value: playlists.length },
      { label: 'Romanization Quizzes', value: quizzes.length },
      { label: 'Handwriting Attempts', value: handwriting.length },
      { label: 'Words Learned', value: words.length },
    ];
  }, [songs, playlists, quizzes, words, handwriting]);

  /* favourite carousels */
  // Define fake image arrays for songs and playlists
  const placeholderImages = [
    'https://t2.genius.com/unsafe/544x544/https%3A%2F%2Fimages.genius.com%2F2203120764ee7832f4868d1424e0afc4.1000x1000x1.png',
    'https://t2.genius.com/unsafe/544x544/https%3A%2F%2Fimages.genius.com%2F0ece8d46f100b7f4ab2d1680e0b501ca.1000x1000x1.png',
    'https://t2.genius.com/unsafe/544x544/https%3A%2F%2Fimages.genius.com%2F96fab843dc59f3be9ec6e577de8552fa.1000x1000x1.png',
    'https://t2.genius.com/unsafe/544x544/https%3A%2F%2Fimages.genius.com%2Fe91d26e761bbb91681df74d537c320f3.1000x1000x1.png',
    'https://t2.genius.com/unsafe/544x544/https%3A%2F%2Fimages.genius.com%2F439f4edc54e1cf83079026369628cf39.1000x1000x1.png',
    'https://t2.genius.com/unsafe/544x544/https%3A%2F%2Fimages.genius.com%2F3e947d45997532b243ccd37bde484492.800x800x1.png',
    'https://t2.genius.com/unsafe/544x544/https%3A%2F%2Fimages.genius.com%2Fb09caf77e18e2a94510f2f95bfbc6752.1000x1000x1.png',
  ];

  // Compute carousel data for favorite songs using the dedicated favoriteSongs state
  const favSongs: MediaItem[] = useMemo(
    () =>
      favoriteSongs.map(({ songId, title, artist, geniusThumbnailUrl }, index) => ({
        id: songId ?? 'unknown-song',
        title: title ?? songId ?? 'Unknown Song',
        subtitle: artist ?? undefined,
        coverUrl:
          geniusThumbnailUrl && geniusThumbnailUrl !== ''
            ? geniusThumbnailUrl
            : placeholderImages[index % placeholderImages.length],
        // Do not set "plays" so that the play count is not rendered for songs
      })),
    [favoriteSongs],
  );

  // Compute carousel data for favorite playlists using the dedicated favoritePlaylists state
  const favPlaylists: MediaItem[] = useMemo(
    () =>
      favoritePlaylists.map(
        ({ playlistId, title, playlistThumbnailUrl, datesPlayed, dateFavorited, isFavorite}, index) => ({
          id: playlistId ?? 'unknown-playlist',
          title: title ?? playlistId ?? 'Unknown Playlist',
          subtitle: undefined,
          coverUrl:
            playlistThumbnailUrl && playlistThumbnailUrl !== ''
              ? playlistThumbnailUrl
              : placeholderImages[index % placeholderImages.length],
          plays: datesPlayed?.length ?? 0,
          dateFavorited: dateFavorited ?? undefined,
          isFavorite: isFavorite ?? false,
        })
      ),
    [favoritePlaylists],
  );

  /* top-3 lists */
  const topSongs = useMemo(() => {
    return getTopAggregatedItems(songs, (s) => s.songId ?? 'unknown-song', (s) => s.datesPlayed?.length ?? 0, 7)
      .map(({ id, plays }) => {
        const song = songs.find(s => s.songId === id);
        return {
          id,
          title: song?.title || id,
          subtitle: song?.artist || undefined,
          coverUrl: song?.geniusThumbnailUrl || '', // fallback as needed
          plays,
          dateFavorited: song?.dateFavorited || undefined,
          isFavorite: song?.isFavorite || false,
        };
      });
  }, [songs]);

  const topPls: MediaItem[] = useMemo(() => {
    return getTopAggregatedItems(
      playlists,
      p => p.playlistId ?? 'unknown-playlist',
      p => p.datesPlayed?.length ?? 0,
      7
    ).map(({ id, plays }, index) => {
      const pl = playlists.find(p => p.playlistId === id);
      return {
        id,
        title:   pl?.title  ?? id,
        // reuse subtitle if you want to show the raw ID under the name:
        subtitle: undefined,
        coverUrl: pl?.playlistThumbnailUrl
                   ?? placeholderImages[index % placeholderImages.length],
        plays,
        dateFavorited: pl?.dateFavorited || undefined,
        isFavorite: pl?.isFavorite || false,
      };
    });
  }, [playlists]);

  const recentWords = useMemo(
    () =>
      [...words]
        .sort(
          (a, b) =>
            new Date(b.dateLearned ?? 0).getTime() -
            new Date(a.dateLearned ?? 0).getTime(),
        )
        .slice(0, 500),
    [words],
  );

  const quizzesForDisplay = useMemo(
    () =>
      getMergedQuizResults(
        quizzes.map((q) => ({ ...q, quizType: 'romanization' })), // <-- ADD QUIZTYPE
        handwriting.map((h) => ({ ...h, quizType: 'handwriting' }))
      ),
    [quizzes, handwriting]
  );

  /* guards */
  if (!user) {
    return (
      <Box mt={6} textAlign="center">
        <Typography sx={{ color: 'black' }}>Please log in to view your dashboard.</Typography>
      </Box>
    );
  }
  if (loading) {
    return (
      <Box display="flex" justifyContent="center" mt={6}>
        <CircularProgress />
      </Box>
    );
  }
  if (error) {
    return (
      <Alert severity="error" sx={{ mt: 4 }}>
        {error}
      </Alert>
    );
  }

  // To fix typing build errors
  const sanitizedQuizzes = quizzesForDisplay.map(q => ({
    ...q,
    quizId: q.quizId ?? 'unknown-quiz',
    language: q.language ?? 'UNK',
    score: q.score ?? 0,
    dateCompleted: q.dateCompleted instanceof Date ? q.dateCompleted.toISOString() : q.dateCompleted ?? null,
  }));

  /* render */
  return (
    <ThemeProvider theme={dashboardTheme}>
      <div className={styles.userStatsPage}>
        <Container maxWidth="xl" className={styles.container}>
          <Typography
            variant="h4"
            className={styles.header}
          >
            Usage Stats
          </Typography>

          <Box className={styles.titleSpacer} />

          <KpiStrip data={kpis} />

          <Box className={styles.kpiSpacer} />

          <Box className={styles.carouselRow}>
            <Box className={styles.carouselCol}>
              <FavoriteSongsCarousel songs={favSongs} />
            </Box>
            <Box className={styles.carouselCol}>
              <FavoritePlaylistsCarousel playlists={favPlaylists} />
            </Box>
          </Box>

          {/* ─────────────── 2) 4 stats panels ─────────────── */}
          <Grid container spacing={3} alignItems="stretch">
            <Grid size={{ xs: 12, sm: 6, md: 3 }} sx={{ maxWidth: 300, minWidth: 260 }} >
              <QuizResultsSection quizzes={sanitizedQuizzes} />
            </Grid>

            <Grid size={{ xs: 12, sm: 6, md: 3 }} sx={{  maxWidth: 400, minWidth: 260 }}>
              <TopSongsChart mediaItems={topSongs} />
            </Grid>

            <Grid size={{ xs: 12, sm: 6, md: 3 }} sx={{  pl: 8 }}>
              <TopPlaylists data={topPls} />
            </Grid>

            <Grid size={{ xs: 12, sm: 6, md: 3 }} sx={{ pl: 2 }}>
              <LearnedWords recent={recentWords} />
            </Grid>
          </Grid>
        </Container>
      </div>
    </ThemeProvider>
  );
};

export default UserStatsPage;
