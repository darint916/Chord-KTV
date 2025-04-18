import React, { useRef } from 'react';
import { useDrag, useDrop } from 'react-dnd';
import { 
  ListItemButton, 
  ListItemText, 
  IconButton, 
  Box, 
  CircularProgress,
  Tooltip,
  Avatar
} from '@mui/material';
import DragIndicatorIcon from '@mui/icons-material/DragIndicator';
import DeleteIcon from '@mui/icons-material/Delete';
import ErrorIcon from '@mui/icons-material/Error';
import './DraggableQueueItem.scss';
import { QueueItem } from '../../contexts/QueueTypes';

interface DraggableQueueItemProps {
  item: QueueItem;
  index: number;
  moveItem: (dragIndex: number, hoverIndex: number) => void;
  onRemove: (id: string) => void;
  onPlay: (item: QueueItem) => void;
  currentPlayingId: string | null;
}

const DraggableQueueItem: React.FC<DraggableQueueItemProps> = ({
  item,
  index,
  moveItem,
  onRemove,
  onPlay,
  currentPlayingId,
}) => {
  const ref = useRef<HTMLDivElement>(null);
  
  const [{ isDragging }, drag] = useDrag({
    type: 'QUEUE_ITEM',
    item: { index },
    collect: (monitor) => ({
      isDragging: monitor.isDragging(),
    }),
  });

  const [, drop] = useDrop({
    accept: 'QUEUE_ITEM',
    hover: (draggedItem: { index: number }, _monitor) => {
      if (!ref.current) return;
      const dragIndex = draggedItem.index;
      const hoverIndex = index;

      if (dragIndex === hoverIndex) return;

      moveItem(dragIndex, hoverIndex);
      draggedItem.index = hoverIndex;
    },
  });

  drag(drop(ref));

  const isLoading = item.apiRequested && !item.processedData && !item.error;
  const hasError = item.error;
  const songImageUrl = item.processedData?.geniusMetaData?.songImageUrl;

  return (
    <div ref={ref} style={{ opacity: isDragging ? 0.5 : 1 }}>
      <ListItemButton 
        onClick={() => !hasError && onPlay(item)}
        className={`queue-item ${currentPlayingId === item.queueId ? 'active-song' : ''} ${hasError ? 'error-item' : ''}`}
        sx={{
          backgroundColor: currentPlayingId === item.queueId 
            ? 'rgba(25, 118, 210, 0.08)' 
            : hasError
              ? 'rgba(255, 0, 0, 0.08)'
              : 'transparent',
          '&:hover': {
            backgroundColor: currentPlayingId === item.queueId 
              ? 'rgba(25, 118, 210, 0.12)' 
              : hasError
                ? 'rgba(255, 0, 0, 0.12)'
                : 'rgba(255, 255, 255, 0.1)'
          }
        }}
        disabled={isLoading}
      >
        {isLoading ? (
          <CircularProgress size={20} sx={{ mr: 2 }} />
        ) : hasError ? (
          <Tooltip title={item.error || "Failed to load song details"}>
            <ErrorIcon color="error" sx={{ mr: 1 }} />
          </Tooltip>
        ) : null}

        {songImageUrl && (
          <Avatar 
            src={songImageUrl} 
            variant="square"
            sx={{ 
              width: 40, 
              height: 40, 
              mr: 2,
              borderRadius: 1
            }}
          />
        )}

        <ListItemText 
          primary={`${index + 1}. ${item.processedData?.title || item.title}`} 
          secondary={item.processedData?.artist || item.artist}
          primaryTypographyProps={{ 
            noWrap: true,
            fontWeight: currentPlayingId === item.queueId ? 'bold' : 'normal',
            color: hasError ? 'error.main' : isLoading ? 'text.disabled' : 'text.primary'
          }}
          secondaryTypographyProps={{ 
            noWrap: true,
            color: hasError ? 'error.main' : isLoading ? 'text.disabled' : 'text.secondary'
          }}
          sx={{ flex: 1 }}
        />

        {currentPlayingId === item.queueId && !hasError && (
          <Box sx={{ 
            width: 8, 
            height: 8, 
            borderRadius: '50%', 
            bgcolor: 'primary.main',
            ml: 1
          }} />
        )}
        {!isLoading && (
          <IconButton
            edge="end"
            aria-label="remove"
            onClick={(e) => {
              e.stopPropagation();
              onRemove(item.queueId);
            }}
            sx={{
              color: hasError ? 'error.main' : 'error.main',
              '&:hover': {
                backgroundColor: 'rgba(255, 0, 0, 0.1)'
              }
            }}
          >
            <DeleteIcon />
          </IconButton>
        )}
      </ListItemButton>
    </div>
  );
};

export default DraggableQueueItem;
