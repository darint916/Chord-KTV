import React, { useRef } from 'react';
import { useDrag, useDrop } from 'react-dnd';
import { ListItemButton, ListItemText, IconButton, Box } from '@mui/material';
import DragIndicatorIcon from '@mui/icons-material/DragIndicator';
import DeleteIcon from '@mui/icons-material/Delete';
import { FullSongResponseDto } from '../../api';
import './DraggableQueueItem.scss';

interface QueueItem extends FullSongResponseDto {
  queueId: string;
}

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
    hover: (draggedItem: { index: number }, monitor) => {
      if (!ref.current) return;
      const dragIndex = draggedItem.index;
      const hoverIndex = index;

      if (dragIndex === hoverIndex) return;

      moveItem(dragIndex, hoverIndex);
      draggedItem.index = hoverIndex;
    },
  });

  drag(drop(ref));

  return (
    <div ref={ref} style={{ opacity: isDragging ? 0.5 : 1 }}>
      <ListItemButton 
        onClick={() => onPlay(item)}
        className={`queue-item ${currentPlayingId === item.queueId ? 'active-song' : ''}`}
        sx={{
          backgroundColor: currentPlayingId === item.queueId ? 'rgba(25, 118, 210, 0.08)' : 'transparent',
          '&:hover': {
            backgroundColor: currentPlayingId === item.queueId 
              ? 'rgba(25, 118, 210, 0.12)' 
              : 'rgba(255, 255, 255, 0.1)'
          }
        }}
      >
        <DragIndicatorIcon 
          className="drag-handle" 
          sx={{ cursor: 'move', mr: 1, opacity: 0.5, '&:hover': { opacity: 1 } }} 
        />
        <ListItemText 
          primary={`${index + 1}. ${item.title}`} 
          secondary={item.artist}
          primaryTypographyProps={{ 
            noWrap: true,
            fontWeight: currentPlayingId === item.queueId ? 'bold' : 'normal'
          }}
          secondaryTypographyProps={{ noWrap: true }}
          sx={{ flex: 1 }}
        />
        {currentPlayingId === item.queueId && (
          <Box sx={{ 
            width: 8, 
            height: 8, 
            borderRadius: '50%', 
            bgcolor: 'primary.main',
            ml: 1
          }} />
        )}
        {currentPlayingId !== item.queueId && (
          <IconButton
            edge="end"
            aria-label="remove"
            onClick={(e) => {
              e.stopPropagation();
              onRemove(item.queueId);
            }}
            sx={{
              color: 'error.main',
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
