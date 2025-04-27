import React from 'react';
import { Paper, Typography, Divider, Box, Button, IconButton, ToggleButton } from '@mui/material';
import { DndProvider } from 'react-dnd';
import { HTML5Backend } from 'react-dnd-html5-backend';
import DraggableQueueItem from '../../components/DraggableQueueItem/DraggableQueueItem';
import { QueueItem } from '../../contexts/QueueTypes';
import ClearAll from '@mui/icons-material/ClearAll';
import './QueueComponent.scss';
import { FixedSizeList as List } from 'react-window';
import AutoSizer from 'react-virtualized-auto-sizer';
import Shuffle from '@mui/icons-material/Shuffle';

interface QueueComponentProps {
  queue: QueueItem[];
  currentPlayingId: string | null;
  setQueue: React.Dispatch<React.SetStateAction<QueueItem[]>>;
  setCurrentPlayingId: React.Dispatch<React.SetStateAction<string | null>>;
  handlePlayFromQueue: (_item: QueueItem) => Promise<void>;
  autoPlayEnabled: boolean;
  setAutoPlayEnabled: React.Dispatch<React.SetStateAction<boolean>>;
}

const QueueComponent: React.FC<QueueComponentProps> = ({
  queue,
  currentPlayingId,
  setQueue,
  setCurrentPlayingId,
  autoPlayEnabled,
  handlePlayFromQueue,
  setAutoPlayEnabled,
}) => {

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

  const handleShuffle = () => {
    const currIdx = queue.findIndex(item => item.queueId === currentPlayingId);
    for (let i = currIdx + 1; i < queue.length - 1; i++) {
      const j = Math.floor(Math.random() * (queue.length - i)) + i;
      [queue[i], queue[j]] = [queue[j], queue[i]];
    }
    setQueue([...queue]);
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

  return (
    <DndProvider backend={HTML5Backend}>
      <Paper elevation={3} className="queue-column">
        <Box display="flex" alignItems="center" justifyContent="space-between" px={2} pt={1}>
          <Typography variant="h6" className="queue-title">
            {currentPlayingId
              ? `Queue (${queue.findIndex(item => item.queueId === currentPlayingId) + 1}/${queue.length})`
              : `Queue (${queue.length})`}
          </Typography>
          <IconButton
            // onClick={handleShuffle}
            aria-label="shuffle queue"
            color="primary"
            size="small"
          >
            <Shuffle />
          </IconButton>
        </Box>
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
