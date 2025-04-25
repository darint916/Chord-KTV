import React from 'react';
import { Box, Typography, Paper, Stack } from '@mui/material';
import Grid from '@mui/material/Grid2';

interface KPI {
  label: string;
  value: number;
}

interface KPIStripProps {
  data: KPI[];
}

const StatPaper: React.FC<KPI> = ({ label, value }) => (
  <Paper sx={{ textAlign: 'center' }}>
    <Typography
      variant="h4"
      sx={{ fontWeight: 600, fontFamily: '"Montserrat", sans-serif' }}
    >
      {value}
    </Typography>
    <Typography variant="body2" color="text.secondary">
      {label}
    </Typography>
  </Paper>
);

const KPIStrip: React.FC<KPIStripProps> = ({ data }) => (
  <>
    {/* xs: horizontal scroll  */}
    <Stack
      direction="row"
      spacing={3}
      sx={{
        display: { xs: 'flex', md: 'none' },
        mb: 3,
        overflowX: 'auto',
        pb: 1,
        px: 1,
        justifyContent: 'center',
      }}
    >
      {data.map((k) => (
        <Box key={k.label} sx={{ minWidth: 160 }}>
          <StatPaper {...k} />
        </Box>
      ))}
    </Stack>

    {/* md+: grid */}
    <Grid
      container
      spacing={3}
      sx={{ 
        display: { xs: 'none', md: 'flex' }, 
        mb: 3,
        justifyContent: 'center'
      }}
    >
      {data.map((k) => (
        <Grid key={k.label} xs={6} md="auto">
          <StatPaper {...k} />
        </Grid>
      ))}
    </Grid>
  </>
);

export default KPIStrip;
