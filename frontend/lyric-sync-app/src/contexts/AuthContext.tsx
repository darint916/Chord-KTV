import React, { useState, useEffect } from 'react';
import { User, AuthContext } from './AuthTypes';
import { clearAuthToken } from '../utils/auth';
import { googleLogout } from '@react-oauth/google';

export const AuthProvider: React.FC<{ children: React.ReactNode }> = ({ children }) => {
  const [user, setUser] = useState<User | null>(() => {
    // Try to get user data from localStorage on initial load
    const savedUser = localStorage.getItem('user');
    return savedUser ? JSON.parse(savedUser) : null;
  });

  const logout = () => {
    clearAuthToken();      // remove JWT
    setUser(null);         // clear user state
    googleLogout();        // revoke Google session
  };

  // Update localStorage whenever user state changes
  useEffect(() => {
    if (user) {
      localStorage.setItem('user', JSON.stringify(user));
    } else {
      localStorage.removeItem('user');
    }
  }, [user]);

  return (
    <AuthContext.Provider value={{ user, setUser, logout }}>
      {children}
    </AuthContext.Provider>
  );
}; 