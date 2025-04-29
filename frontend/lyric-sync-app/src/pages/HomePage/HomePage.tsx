import React, { useState } from 'react';
import {
  Box,
  Typography,
  TextField,
  IconButton,
  Alert,
  Paper,
  Container,
  Button,
  Skeleton
} from '@mui/material';
import { useNavigate } from 'react-router-dom';
import { useSong } from '../../contexts/SongContext';
import SearchIcon from '@mui/icons-material/Search';
import './HomePage.scss';
import { songApi } from '../../api/apiClient';
import logo from '../../assets/chordktv.png';
import { v4 as uuidv4 } from 'uuid';
import { QueueItem } from '../../contexts/QueueTypes';
import { extractYouTubeVideoId, extractPlaylistId } from './HomePageHelpers';
import GeniusHitsCarousel from '../../components/GeniusHitsCarousel/GeniusHitsCarousel';
import type { GeniusHit } from '../../api';

const HomePage: React.FC = () => {
  const [songName, setSongName] = useState('');
  const [artistName, setArtistName] = useState('');
  const [error, setError] = useState('');
  const [isLoading, setIsLoading] = useState(false);
  const [playlistLoading, setPlaylistLoading] = useState(false);
  const navigate = useNavigate();
  const { setSong, queue, setQueue, setCurrentPlayingId } = useSong();
  const [playlistUrl, setPlaylistUrl] = useState('');
  const [lyrics, setLyrics] = useState('');
  const [youtubeUrl, setYoutubeUrl] = useState('');
  const [geniusHits, setGeniusHits] = useState<GeniusHit[]>([]);

  const handleLoadPlaylist = async () => {
    if (!playlistUrl.trim()) {
      setError('Invalid Playlist URL.');
      return;
    }

    setPlaylistLoading(true);
    setError('');

    try {
      const playlistId = extractPlaylistId(playlistUrl);
      if (!playlistId) { throw new Error('Invalid YouTube playlist URL'); }

      const response = await songApi.apiYoutubePlaylistsPlaylistIdGet({ playlistId });
      const videos = response.videos || [];
      if (videos.length === 0) { throw new Error('This playlist contains no videos'); }

      // Create queue items with basic info
      const newQueue = videos.map(video => ({
        queueId: uuidv4(),
        title: video.title || 'Unknown Track',
        artist: video.artist || 'Unknown Artist',
        youTubeId: extractYouTubeVideoId(video.url) || '',
        lyrics: '',
        status: 'pending' as const
      }));

      // Set the queue with new songs
      setQueue(newQueue);

      // Immediately process the first song
      const firstSong = newQueue[0];
      try {
        const processed = await songApi.apiSongsMatchPost({
          fullSongRequestDto: {
            title: firstSong.title,
            artist: firstSong.artist,
            youTubeId: firstSong.youTubeId,
            lyrics: firstSong.lyrics
          }
        });
        if (firstSong.youTubeId) { //override with user youtube vid
          processed.youTubeId = firstSong.youTubeId;
        }
        // Update queue with processed data for first song
        setQueue(prev => prev.map(item =>
          item.queueId === firstSong.queueId
            ? { ...item, processedData: processed, status: 'loaded' as const, imageUrl: processed.geniusMetaData?.songImageUrl ?? '' }
            : item
        ));

        // Set as current song with full data
        setCurrentPlayingId(firstSong.queueId);
        setSong(processed);
      } catch (err) {
        const errorMessage = err instanceof Error
          ? `Failed to process first song. Error message from OpenAPI stub call: ${err.message}`
          : 'Failed to process first song';
        setError(errorMessage);
        // Fallback to basic info if processing fails
        setCurrentPlayingId(firstSong.queueId);
        setSong({
          title: firstSong.title,
          artist: firstSong.artist,
          youTubeId: firstSong.youTubeId,
          lrcLyrics: '',
          lrcRomanizedLyrics: '',
          lrcTranslatedLyrics: ''
        });
      }

      navigate('/play-song');
    } catch (err) {
      const errorMessage = err instanceof Error
        ? `Failed to load playlist. Error message from OpenAPI stub call: ${err.message}`
        : 'Failed to load playlist';
      setError(errorMessage);
    } finally {
      setPlaylistLoading(false);
    }
  };

  const handleSearch = async () => {
    // If only a YouTube URL is provided, skip Genius search and call match directly
    if (youtubeUrl.trim()) {
      const youTubeId = extractYouTubeVideoId(youtubeUrl.trim());
      if (!youTubeId) {
        setError('Invalid YouTube URL.');
        return;
      }

      setIsLoading(true);
      setError('');
      try {
        const response = await songApi.apiSongsMatchPost({
          fullSongRequestDto: {
            title: songName,
            artist: artistName,
            youTubeId,
            lyrics,
          }
        });
        // preserve the original video ID
        response.youTubeId = youTubeId;

        const newQueueItem: QueueItem = {
          ...response,
          queueId: uuidv4(),
          title: response.title ?? songName,
          artist: response.artist ?? artistName,
          youTubeId,
          lyrics,
          status: 'loaded',
          imageUrl: response.geniusMetaData?.songImageUrl ?? ''
        };
        setQueue([newQueueItem, ...queue]);
        setCurrentPlayingId(newQueueItem.queueId);
        setSong(response);
        navigate('/play-song');
      } catch (err) {
        setError('Search failed. Please try again. Error: ' + err);
      } finally {
        setIsLoading(false);
      }
      return;
    }

    // Otherwise, build a free-text Genius search query
    const queryParts = [songName, artistName, lyrics]
      .map((s) => s.trim())
      .filter(Boolean);
    const searchQuery = queryParts.join(' ');

    if (!searchQuery) {
      setError('Please enter at least one field to search.');
      return;
    }

    setIsLoading(true);
    setGeniusHits([]);

    try {
      const res = await songApi.apiSongsGeniusSearchGet({ searchQuery });

      const hits = res ?? [];
      if (hits.length === 0) {
        setError('No results found.');
        return;
      }

      setGeniusHits(hits);
    } catch (err) {
      setError('Search failed. Please try again. Error: ' + err);
    } finally {
      setIsLoading(false);
    }
  };

  const handleResultSelect = async (hitDto: GeniusHit) => {
    const hit = hitDto.result;
    if (!hit) {
      setError('Selected song is missing details.');
      return;
    }
    const title = hit.title ?? '';
    const artist =
      (hit as { primaryArtistNames?: string; primary_artist_names?: string }).primaryArtistNames ||
      (hit as { primaryArtistNames?: string; primary_artist_names?: string }).primary_artist_names ||
      '';

    setIsLoading(true);
    setError('');

    try {
      // grab the same YouTube ID & lyrics from state
      const youTubeId = extractYouTubeVideoId(youtubeUrl.trim()) || '';

      const response = await songApi.apiSongsMatchPost({
        fullSongRequestDto: {
          title,
          artist,
          lyrics,
          youTubeId,
        },
      });

      const newQueueItem: QueueItem = {
        ...response,
        title: response.title ?? title,
        artist: response.artist ?? artist,
        youTubeId,
        queueId: uuidv4(),
        lyrics,
        status: 'loaded',
        imageUrl:
          response.geniusMetaData?.songImageUrl ??
          hit.songArtImageUrl ??
          hit.headerImageUrl ??
          '',
      };

      setQueue([newQueueItem, ...queue]);
      setCurrentPlayingId(newQueueItem.queueId);
      setSong(response);

      // Clear results so the carousel disappears next time
      setGeniusHits([]);

      navigate('/play-song');
    } catch (err) {
      const errorMessage = err instanceof Error
        ? `Search failed. Please try again. Error message from OpenAPI stub call: ${err.message}`
        : 'Search failed. Please try again.';
      setError(errorMessage);
    } finally {
      setIsLoading(false);
    }
  };

  const handleMatchSearch = async () => {
    setIsLoading(true);
    setError('');

    // extract YouTube ID if present
    let youTubeId = '';
    if (youtubeUrl.trim()) {
      const id = extractYouTubeVideoId(youtubeUrl.trim());
      if (!id) {
        setError('Invalid YouTube URL.');
        setIsLoading(false);
        return;
      }
      youTubeId = id;
    }

    try {
      // always POST to /songs/match
      const response = await songApi.apiSongsMatchPost({
        fullSongRequestDto: {
          title: songName,
          artist: artistName,
          lyrics,
          youTubeId,
        },
      });

      // preserve the ID
      response.youTubeId = youTubeId;

      // build a QueueItem just like handleSearch's YouTube branch
      const newQueueItem: QueueItem = {
        ...response,
        queueId: uuidv4(),
        title: response.title ?? songName,
        artist: response.artist ?? artistName,
        youTubeId,
        lyrics,
        status: 'loaded',
        imageUrl: response.geniusMetaData?.songImageUrl ?? '',
      };

      setQueue([newQueueItem, ...queue]);
      setCurrentPlayingId(newQueueItem.queueId);
      setSong(response);
      navigate('/play-song');
    } catch (err) {
      const msg = err instanceof Error ? err.message : String(err);
      setError('Match failed. Please try again. Error: ' + msg);
    } finally {
      setIsLoading(false);
    }
  };

  const handleKeyDown = (event: React.KeyboardEvent<HTMLDivElement>) => {
    if (event.key === 'Enter') {
      if (playlistUrl) {
        handleLoadPlaylist();
      } else {
        handleSearch();
      }
    }
  };

  return (
    <Box className="home-page">
      <Container maxWidth="md">
        <img src={logo} alt="Logo" className="resized-image" />
        {error && (
          <Alert severity="error" className="error-alert">
            {error}
          </Alert>
        )}

        {/* Search Section */}
        <Paper elevation={3} className="search-section">
          <Typography variant="h5" className="section-title">
            Search for a Song
          </Typography>
          <Box display="flex" alignItems="center" gap={2}>
            <Box
              display="grid"
              gridTemplateColumns="1fr 1fr"
              gap={2}
              flexGrow={1}
            >
              {isLoading ? (
                <>
                  <Skeleton className="skeleton-input" variant="rectangular" />
                  <Skeleton className="skeleton-input" variant="rectangular" />
                  <Skeleton className="skeleton-input" variant="rectangular" />
                  <Skeleton className="skeleton-input" variant="rectangular" />
                </>
              ) : (
                <>
                  <TextField
                    label="Song Name"
                    variant="filled"
                    value={songName}
                    disabled={isLoading || playlistLoading}
                    onKeyDown={handleKeyDown}
                    onChange={(e) => setSongName(e.target.value)}
                    className="search-input"
                    fullWidth
                  />
                  <TextField
                    label="Artist Name"
                    variant="filled"
                    value={artistName}
                    disabled={isLoading || playlistLoading}
                    onKeyDown={handleKeyDown}
                    onChange={(e) => setArtistName(e.target.value)}
                    className="search-input"
                    fullWidth
                  />
                  <TextField
                    label="Lyrics"
                    variant="filled"
                    value={lyrics}
                    disabled={isLoading || playlistLoading}
                    onKeyDown={handleKeyDown}
                    onChange={(e) => setLyrics(e.target.value)}
                    className="search-input"
                    fullWidth
                  />
                  <TextField
                    label="YouTube URL"
                    variant="filled"
                    value={youtubeUrl}
                    disabled={isLoading || playlistLoading}
                    onKeyDown={handleKeyDown}
                    onChange={(e) => setYoutubeUrl(e.target.value)}
                    className="search-input"
                    fullWidth
                  />
                </>
              )}
            </Box>
            <IconButton
              aria-label="search"
              onClick={handleSearch}
              disabled={isLoading || playlistLoading}
              className={`search-button ${isLoading ? 'loading' : ''}`}
              size="large"
            >
              <SearchIcon />
            </IconButton>
          </Box>
        </Paper>

        {/* SHOW GENIUS CANDIDATES WHEN PRESENT */}
        {geniusHits.length > 0 && (
          <GeniusHitsCarousel
            hits={geniusHits}
            onSelect={handleResultSelect}
            onMatchSearch={handleMatchSearch}
          />
        )}

        {/* OR Divider */}
        <Box className="or-divider">
          <Paper elevation={3} className="or-paper">
            <Typography variant="body1" className="or-text">
              OR
            </Typography>
          </Paper>
        </Box>

        {/* YouTube Playlist Section */}
        <Paper elevation={3} className="playlist-section">
          <Typography variant="h5" className="section-title">
            Load a YouTube Playlist
          </Typography>
          <div className='playlist-url-input'>
            {playlistLoading ? (
              <Skeleton
                className="skeleton-input"
                variant="rectangular"
                height={56}
                width="100%"
              />
            ) : (
              <TextField
                fullWidth
                label="Enter YouTube Playlist URL"
                variant="filled"
                value={playlistUrl}
                disabled={isLoading || playlistLoading}
                onKeyDown={handleKeyDown}
                onChange={(e) => setPlaylistUrl(e.target.value)}
              />
            )}
            <Button
              variant="contained"
              onClick={handleLoadPlaylist}
              disabled={isLoading || playlistLoading}
            >
              Load Playlist
            </Button>
          </div>
        </Paper>
      </Container>
    </Box>
  );
};

export default HomePage;
