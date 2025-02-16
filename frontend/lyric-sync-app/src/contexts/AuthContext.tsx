import React, { createContext, useState, useEffect, useContext } from 'react';

// Define User type in the same file for now
export interface User {
  id: string;
  name: string;
  email: string;
  picture: string;
  idToken: string;
}

interface AuthContextType {
  user: User | null;
  setUser: (_user: User | null) => void;
}

const AuthContext = createContext<AuthContextType | undefined>(undefined);

// Prefix unused 'user' with underscore
export const AuthProvider: React.FC<{ children: React.ReactNode }> = ({ children }) => {
  const [user, setUser] = useState<User | null>(() => {
    // Try to get user data from localStorage on initial load
    const savedUser = localStorage.getItem('user');
    return savedUser ? JSON.parse(savedUser) : null;
  });

  // Update localStorage whenever user state changes
  useEffect(() => {
    if (user) {
      localStorage.setItem('user', JSON.stringify(user));
    } else {
      localStorage.removeItem('user');
    }
  }, [user]);

  return (
    <AuthContext.Provider value={{ user, setUser }}>
      {children}
    </AuthContext.Provider>
  );
};

export const useAuth = () => {
  const context = useContext(AuthContext);
  if (context === undefined) {
    throw new Error('useAuth must be used within an AuthProvider');
  }
  return context;
}; 