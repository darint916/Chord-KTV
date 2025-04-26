import { createContext, useContext } from 'react';

// Define User type
export interface User {
  id: string;
  name: string;
  email: string;
  picture: string;
  idToken: string;
}

export interface AuthContextType {
  user: User | null;
  setUser: (_u: User | null) => void;
  logout: () => void;
}

export const AuthContext = createContext<AuthContextType | undefined>(undefined);

export const useAuth = () => {
  const context = useContext(AuthContext);
  if (context === undefined) {
    throw new Error('useAuth must be used within an AuthProvider');
  }
  return context;
}; 