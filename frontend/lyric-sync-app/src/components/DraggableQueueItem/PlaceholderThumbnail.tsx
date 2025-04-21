import { Box } from '@mui/material';
import MusicNoteIcon from '@mui/icons-material/MusicNote';
import './PlaceholderThumbnail.scss'

const PlaceholderThumbnail = () => {
  return (
    <Box className="placeholder-thumbnail">
      <MusicNoteIcon fontSize="medium" color="disabled" />
    </Box>
  );
};

export default PlaceholderThumbnail;
