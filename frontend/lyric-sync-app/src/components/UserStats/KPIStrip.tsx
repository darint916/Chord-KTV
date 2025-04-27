import React from 'react';
import { Box, Typography, Paper } from '@mui/material';
import styles from './KpiStrip.module.scss';

interface Kpi {
  label: string;
  value: number;
}

interface KpiStripProps {
  data: Kpi[];
}

const StatPaper: React.FC<Kpi> = ({ label, value }) => (
  <Paper className={styles.statPaper}>
    <Typography variant="h4" className={styles.statValue}>
      {value}
    </Typography>
    <Typography variant="body2" color="text.secondary">
      {label}
    </Typography>
  </Paper>
);

const KpiStrip: React.FC<KpiStripProps> = ({ data }) => (
  <>
    {/* Mobile view: horizontal scroll container */}
    <Box className={styles.mobileContainer}>
      {data.map((k) => (
        <Box key={k.label} className={styles.mobileItem}>
          <StatPaper {...k} />
        </Box>
      ))}
    </Box>

    {/* Desktop view: flex container with wrapping */}
    <Box className={styles.desktopContainer}>
      {data.map((k) => (
        <Box key={k.label} className={styles.desktopItem}>
          <StatPaper {...k} />
        </Box>
      ))}
    </Box>
  </>
);

export default KpiStrip;
