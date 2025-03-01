import React from 'react';
import { GoogleLogin, CredentialResponse } from '@react-oauth/google';
import { jwtDecode } from 'jwt-decode';
import { useAuth } from '../../contexts/authTypes';
import { Configuration, UserApi } from '../../api';

interface GooglePayload {
  sub: string;
  name: string;
  email: string;
  picture: string;
}

// Initialize API client
const userApi = new UserApi(
  new Configuration({
    basePath: import.meta.env.VITE_API_URL || 'http://localhost:5259',
  })
);

const GoogleAuth: React.FC = () => {
  const { setUser } = useAuth();

  const handleLogin = async (credentialResponse: CredentialResponse) => {
    const decoded: GooglePayload = jwtDecode(credentialResponse.credential ?? '');
    
    try {
      // Use the OpenAPI generated client instead of axios
      await fetch(`${import.meta.env.VITE_API_URL || 'http://localhost:5259'}/api/auth/google`, {
        method: 'POST',
        headers: {
          'Authorization': `Bearer ${credentialResponse.credential}`
        }
      });

      setUser({
        id: decoded.sub,
        name: decoded.name,
        email: decoded.email,
        picture: decoded.picture,
        idToken: credentialResponse.credential ?? ''
      });
    } catch {
      handleLoginError();
    }
  };

  const handleLoginError = () => {
    // Handle login error silently or show a user-friendly message
    setUser(null);
  };

  return (
    <GoogleLogin
      onSuccess={handleLogin}
      onError={handleLoginError}
    />
  );
};

export default GoogleAuth; 