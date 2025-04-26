import React from 'react';
import { Box, Typography, Paper } from '@mui/material';
import styles from './KPIStrip.module.scss';

interface KPI {
  label: string;
  value: number;
}

interface KPIStripProps {
  data: KPI[];
}

const StatPaper: React.FC<KPI> = ({ label, value }) => (
  <Paper className={styles.statPaper}>
    <Typography variant="h4" className={styles.statValue}>
      {value}
    </Typography>
    <Typography variant="body2" color="text.secondary">
      {label}
    </Typography>
  </Paper>
);

const KPIStrip: React.FC<KPIStripProps> = ({ data }) => (
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

export default KPIStrip;
