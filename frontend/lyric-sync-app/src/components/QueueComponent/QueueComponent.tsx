import React, { useState } from 'react';
import { Paper, Typography, Divider, Box, Button, Alert } from '@mui/material';
import { DndProvider } from 'react-dnd';
import { HTML5Backend } from 'react-dnd-html5-backend';
import DraggableQueueItem from '../../components/DraggableQueueItem/DraggableQueueItem';
import { QueueItem } from '../../contexts/QueueTypes';
import { useSong } from '../../contexts/SongContext';
import { songApi } from '../../api/apiClient';
import ClearAll from '@mui/icons-material/ClearAll';
import './QueueComponent.scss';
import { FixedSizeList as List } from 'react-window';
import AutoSizer from 'react-virtualized-auto-sizer';

interface QueueComponentProps {
  queue: QueueItem[];
  currentPlayingId: string | null;
  setQueue: React.Dispatch<React.SetStateAction<QueueItem[]>>;
  setCurrentPlayingId: React.Dispatch<React.SetStateAction<string | null>>;
}

const QueueComponent: React.FC<QueueComponentProps> = ({
  queue,
  currentPlayingId,
  setQueue,
  setCurrentPlayingId,
}) => {
  const [error, setError] = useState('');
  const { setSong } = useSong();

  const moveQueueItem = (dragIndex: number, hoverIndex: number) => {
    setQueue((prevQueue: QueueItem[]) => {
      const newQueue = [...prevQueue];
      const [removed] = newQueue.splice(dragIndex, 1);
      newQueue.splice(hoverIndex, 0, removed);
      return newQueue;
    });
  };

  const removeFromQueue = (queueId: string) => {
    const newQueue = queue.filter(item => item.queueId !== queueId);
    setQueue(newQueue);
  };

  const clearQueue = () => {
    if (!currentPlayingId) {
      setQueue([]);
      setCurrentPlayingId(null);
      return;
    }

    const currentSong = queue.find(item => item.queueId === currentPlayingId);
    if (currentSong) {
      setQueue([currentSong]);
    } else {
      setQueue([]);
      setCurrentPlayingId(null);
    }
  };

  const handlePlayFromQueue = async (item: QueueItem) => {
    setError('');

    try {
      if (!item.apiRequested) {
        setQueue(prevQueue => prevQueue.map(queueItem =>
          queueItem.queueId === item.queueId
            ? { ...queueItem, apiRequested: true, error: undefined }
            : queueItem
        ));

        const response = await songApi.apiSongsMatchPost({
          fullSongRequestDto: {
            title: item.title,
            artist: item.artist,
            youTubeId: item.youTubeId || '',
            lyrics: item.lyrics || ''
          }
        });
        if (item.youTubeId) {
          response.youTubeId = item.youTubeId;
        }
        const processedData = {
          title: response.title,
          artist: response.artist,
          youTubeId: response.youTubeId,
          lrcLyrics: response.lrcLyrics,
          lrcRomanizedLyrics: response.lrcRomanizedLyrics,
          lrcTranslatedLyrics: response.lrcTranslatedLyrics,
          geniusMetaData: response.geniusMetaData,
          id: response.id
        };

        setQueue(prevQueue => prevQueue.map(queueItem =>
          queueItem.queueId === item.queueId
            ? { ...queueItem, processedData, apiRequested: true }
            : queueItem
        ));

        setCurrentPlayingId(item.queueId);
        setSong(processedData);
      } else {
        const playbackData = item.processedData || {
          title: item.title,
          artist: item.artist,
          youTubeId: item.youTubeId || '',
          lrcLyrics: '',
          lrcRomanizedLyrics: '',
          lrcTranslatedLyrics: ''
        };

        setCurrentPlayingId(item.queueId);
        setSong(playbackData);
      }
    } catch (err) {
      const errorMessage = err instanceof Error ? err.message : 'Failed to load song details';
      setQueue(prevQueue => prevQueue.map(queueItem =>
        queueItem.queueId === item.queueId
          ? { ...queueItem, error: errorMessage }
          : queueItem
      ));

      setCurrentPlayingId(item.queueId);
      setSong({
        title: item.title,
        artist: item.artist,
        youTubeId: item.youTubeId || '',
        lrcLyrics: '',
        lrcRomanizedLyrics: '',
        lrcTranslatedLyrics: ''
      });
    }
  };
  
  return (
    <DndProvider backend={HTML5Backend}>
      <Paper elevation={3} className="queue-column">
        {error && <Alert severity="error">{error}</Alert>}
        <Typography variant="h6" className="queue-title" align="center">
          {currentPlayingId
            ? `Queue (${queue.findIndex(item => item.queueId === currentPlayingId) + 1}/${queue.length})`
            : `Queue (${queue.length})`}
        </Typography>
        <Divider variant="fullWidth" className="queue-divider" />
        
        {/* Main content area with fixed height */}
        <Box className="queue-list-container">
          <AutoSizer>
            {({ height, width }) => (
              <List className="queue-list"
                height={height}
                itemCount={queue.length}
                itemSize={72}
                width={width}
              >
                {({ index, style }) => {
                  const item = queue[index];
                  return (
                    <div style={style}>
                      <DraggableQueueItem
                        item={item}
                        index={index}
                        moveItem={moveQueueItem}
                        onRemove={removeFromQueue}
                        onPlay={handlePlayFromQueue}
                        currentPlayingId={currentPlayingId}
                      />
                      {index < queue.length - 1 && <Divider />}
                    </div>
                  );
                }}
              </List>
            )}
          </AutoSizer>
        </Box>
        
        {/* Button container with fixed position at bottom */}
        <Box className="queue-button-container">
          <Button
            variant="outlined"
            onClick={clearQueue}
            startIcon={<ClearAll />}
            fullWidth
          >
            Clear Queue
          </Button>
        </Box>
      </Paper>
    </DndProvider>
  );

};

export default QueueComponent;
