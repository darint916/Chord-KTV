import React from 'react';
import { BrowserRouter as Router, Route, Routes } from 'react-router-dom';
import HomePage from './pages/HomePage';
import SongPlayerPage from './pages/SongPlayerPage';
import HandwritingPage from './pages/HandwritingPage';
import AppBarComponent from './components/AppBarComponent';

const App: React.FC = () => {
  return (
    <Router>
      <AppBarComponent />
      <Routes>
        <Route path="/" element={<HomePage />} />
        <Route path="/play-song" element={<SongPlayerPage />} />
        <Route path="/canvas" element={<HandwritingPage />} />
      </Routes>
    </Router>
  );
};

export default App;