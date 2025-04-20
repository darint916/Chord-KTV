import { Box } from '@mui/material';
import MusicNoteIcon from '@mui/icons-material/MusicNote';

const PlaceholderThumbnail = () => {
  return (
    <Box
      sx={{
        width: 40,
        height: 40,
        backgroundColor: 'grey.200',
        display: 'flex',
        alignItems: 'center',
        justifyContent: 'center',
        borderRadius: 1,
        marginRight: 2
      }}
    >
      <MusicNoteIcon fontSize="medium" color="disabled" />
    </Box>
  );
};

export default PlaceholderThumbnail;
