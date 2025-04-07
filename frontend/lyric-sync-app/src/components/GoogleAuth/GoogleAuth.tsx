import React from 'react';
import { GoogleLogin, CredentialResponse } from '@react-oauth/google';
import { jwtDecode } from 'jwt-decode';
import { useAuth } from '../../contexts/AuthTypes';
import { AuthApi, Configuration } from '../../api';

// Initialize API client
const authApi = new AuthApi(
  new Configuration({
    basePath: import.meta.env.VITE_API_URL || 'http://localhost:5259',
  })
);

interface GooglePayload {
  sub: string;
  name: string;
  email: string;
  picture: string;
}

const GoogleAuth: React.FC = () => {
  const { setUser } = useAuth();

  const handleLogin = async (credentialResponse: CredentialResponse) => {
    const decoded: GooglePayload = jwtDecode(credentialResponse.credential ?? '');
    
    try {
      // Use the AuthApi stub instead of a direct fetch call
      await authApi.apiAuthGooglePost(credentialResponse.credential ?? '');

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