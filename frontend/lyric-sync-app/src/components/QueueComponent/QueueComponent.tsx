import React from 'react';
import { Paper, Typography, Divider, Box, Button } from '@mui/material';
import { DndProvider } from 'react-dnd';
import { HTML5Backend } from 'react-dnd-html5-backend';
import DraggableQueueItem from '../../components/DraggableQueueItem/DraggableQueueItem';
import { QueueItem } from '../../contexts/QueueTypes';
import ClearAll from '@mui/icons-material/ClearAll';
import './QueueComponent.scss';
import { FixedSizeList as List } from 'react-window';
import AutoSizer from 'react-virtualized-auto-sizer';

interface QueueComponentProps {
  queue: QueueItem[];
  currentPlayingId: string | null;
  setQueue: React.Dispatch<React.SetStateAction<QueueItem[]>>;
  setCurrentPlayingId: React.Dispatch<React.SetStateAction<string | null>>;
  handlePlayFromQueue: (_item: QueueItem) => Promise<void>;
}

const QueueComponent: React.FC<QueueComponentProps> = ({
  queue,
  currentPlayingId,
  setQueue,
  setCurrentPlayingId,
  handlePlayFromQueue
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
