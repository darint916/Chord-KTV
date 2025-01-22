import React from 'react';
import { Link } from 'react-router-dom';

const HomePage = () => {
  return (
    <div className="home-container" 
    style={{ 
      display: 'flex', 
      justifyContent: 'center', 
      alignItems: 'center', 
      flexDirection: 'column', 
      height: '100vh', 
      backgroundColor: '#8a2be2', 
      color: 'white', 
      fontFamily: 'Arial' 
      }}
    >
      <h1>Welcome to Chord KTV!</h1>
      <p>Click below to play a song with synced lyrics:</p>
      <Link to="/play-song">
        <button style={{ padding: '10px 20px', fontSize: '16px', backgroundColor: '#f1f1f1', color: '#000', border: 'none', cursor: 'pointer', borderRadius: '5px' }}>Play Song</button>
      </Link>
    </div>
  );
};

export default HomePage;
