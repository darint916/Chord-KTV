import { createTheme } from '@mui/material/styles';

const dashboardTheme = createTheme({
  palette: {
    mode: 'light',
    primary: { main: '#0052CC' },
    secondary: { main: '#FFAB00' },
    success: { main: '#28A745' },
    warning: { main: '#FFC107' },
    error:   { main: '#DC3545' },
    background: {
      default: '#e0e7ff',
      paper:   '#FFFFFF',
    },
    text: {
      primary: '#2C3138',
      secondary: '#5A6069',
    },
  },
  shape: { borderRadius: 8 },
  spacing: 8,
  typography: {
    fontFamily: '"Open Sans", sans-serif',
    h1: { fontFamily: '"Montserrat", sans-serif', fontWeight: 600, fontSize: 32 },
    h2: { fontFamily: '"Montserrat", sans-serif', fontWeight: 600, fontSize: 24 },
    h3: { fontFamily: '"Montserrat", sans-serif', fontWeight: 600, fontSize: 18 },
    h4: { fontFamily: '"Montserrat", sans-serif', fontWeight: 600 },
    h5: { fontFamily: '"Montserrat", sans-serif', fontWeight: 500 },
    h6: { fontFamily: '"Montserrat", sans-serif', fontWeight: 500 },
  },
  components: {
    MuiPaper: {
      styleOverrides: {
        root: {
          padding: '24px',
          boxShadow: '0 4px 16px rgba(0,0,0,0.05)',
        },
      },
    },
    MuiFocusVisible: {
      styleOverrides: {
        root: { outline: '2px dashed #0052CC !important' },
      },
    },
  },
});

export default dashboardTheme;
