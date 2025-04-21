import React, { useRef } from 'react';
import { useDrag, useDrop } from 'react-dnd';
import {
  ListItemButton,
  ListItemText,
  IconButton,
  Box,
  CircularProgress,
  Tooltip,
  Avatar,
  Typography
} from '@mui/material';
import DeleteIcon from '@mui/icons-material/Delete';
import ErrorIcon from '@mui/icons-material/Error';
import './DraggableQueueItem.scss';
import { QueueItem } from '../../contexts/QueueTypes';
import PlaceholderThumbnail from './PlaceholderThumbnail';

interface DraggableQueueItemProps {
  item: QueueItem;
  index: number;
  moveItem: (_dragIndex: number, _hoverIndex: number) => void;
  onRemove: (_id: string) => void;
  onPlay: (_item: QueueItem) => void;
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
    hover: (draggedItem: { index: number }) => {
      if (!ref.current) { return; }
      const dragIndex = draggedItem.index;
      const hoverIndex = index;

      if (dragIndex === hoverIndex) { return; }

      moveItem(dragIndex, hoverIndex);
      draggedItem.index = hoverIndex;
    },
  });

  drag(drop(ref));

  const isLoading = item.apiRequested && !item.processedData && !item.error;
  const hasError = !!item.error;
  const songImageUrl = item.processedData?.geniusMetaData?.songImageUrl;
  const isCurrentSong = currentPlayingId === item.queueId;
  const isPending = !item.apiRequested;

  return (
    <div
      ref={ref}
      className={`draggable-queue-item ${!isDragging ? 'not-dragging' : ''}`}
    >
      <ListItemButton
        onClick={() => onPlay(item)}
        className={`queue-item 
          ${isCurrentSong ? 'active-song' : ''} 
          ${hasError ? 'error-item' : ''} 
          ${isPending ? 'not-requested' : ''}`}
        disabled={isLoading}
      >
        {hasError && (
          <Tooltip
            title={
              <Box>
                <Typography variant="body2" color="inherit">
                  {item.error}
                </Typography>
              </Box>
            }
            arrow
          >
            <ErrorIcon color="error" sx={{ mr: 1 }} />
          </Tooltip>
        )}

        {songImageUrl ? (
          <Avatar src={songImageUrl} variant="square" />
        ) : (
          <PlaceholderThumbnail />
        )}

        <ListItemText
          primary={`${index + 1}. ${item.processedData?.title || item.title}`}
          secondary={item.processedData?.artist || item.artist}
          primaryTypographyProps={{
            noWrap: true,
            className: isCurrentSong ? 'active' : '',
            color: hasError ? 'error.main' : isLoading ? 'text.disabled' : 'inherit'
          }}
          secondaryTypographyProps={{
            noWrap: true,
            color: hasError ? 'error.main' : isLoading ? 'text.disabled' : 'inherit'
          }}
        />

        {isCurrentSong && !hasError && (
          <div className="now-playing-indicator" />
        )}

        {isLoading ? (
          <CircularProgress size={20} />
        ) : (
          !isCurrentSong && (
            <IconButton
              edge="end"
              aria-label="remove"
              onClick={(e) => {
                e.stopPropagation();
                onRemove(item.queueId);
              }}
            >
              <DeleteIcon />
            </IconButton>
          )
        )}
      </ListItemButton>
    </div>
  );
};

export default DraggableQueueItem;
