import React from 'react';
import {
  Avatar,
  Box,
  Button,
  List,
  ListItem,
  ListItemAvatar,
  ListItemText,
  Paper,
  Typography,
} from '@mui/material';
import QueueMusicIcon from '@mui/icons-material/QueueMusic';

interface PlaylistDatum {
  id: string;
  plays: number;
}

interface TopPlaylistsProps {
  data: PlaylistDatum[];
}

const TopPlaylists: React.FC<TopPlaylistsProps> = ({ data }) => (
  <Paper>
    <Box display="flex" justifyContent="space-between" alignItems="center" mb={1}>
      <Typography variant="h6">Top&nbsp;Playlists</Typography>
      {/* <Box ml={2}>
        <Button variant="contained" size="small">
          Add&nbsp;Playlist
        </Button>
      </Box> */}
    </Box>
    {data.length === 0 ? (
      <Typography variant="body2" color="text.secondary">
        No playlist activity yet.
      </Typography>
    ) : (
      <List dense disablePadding>
        {data.map((p) => (
          <ListItem key={p.id}>
            <ListItemAvatar>
              <Avatar variant="rounded" sx={{ bgcolor: 'secondary.main' }}>
                <QueueMusicIcon />
              </Avatar>
            </ListItemAvatar>
            <ListItemText
              primary={p.id}
              secondary={`${p.plays} plays`}
              secondaryTypographyProps={{ color: 'text.secondary' }}
            />
          </ListItem>
        ))}
      </List>
    )}
  </Paper>
);

export default TopPlaylists;
