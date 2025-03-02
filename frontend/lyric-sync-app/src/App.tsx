import React from 'react';
import { BrowserRouter as Router, Route, Routes } from 'react-router-dom';
import HomePage from './pages/HomePage/HomePage';
import SongPlayerPage from './pages/SongPlayerPage/SongPlayerPage';
import HandwritingPage from './pages/HandwritingPage/HandwritingPage';
import AppBarComponent from './components/AppBarComponent/AppBarComponent';
import { GoogleOAuthProvider } from '@react-oauth/google';
import { AuthProvider } from './contexts/AuthContext';
import { GOOGLE_CLIENT_ID } from './config/GoogleAuth';

const App: React.FC = () => {
  return (
    <GoogleOAuthProvider clientId={GOOGLE_CLIENT_ID}>
      <AuthProvider>
        <Router>
          <AppBarComponent />
          <Routes>
            <Route path="/" element={<HomePage />} />
            <Route path="/play-song" element={<SongPlayerPage />} />
            <Route path="/canvas" element={<HandwritingPage />} />
          </Routes>
        </Router>
      </AuthProvider>
    </GoogleOAuthProvider>
  );
};

export default App;