import React from 'react';
import { GoogleLogin, CredentialResponse } from '@react-oauth/google';
import { jwtDecode } from 'jwt-decode';
import { useAuth } from '../../contexts/AuthTypes';
import { Configuration } from '../../api';
import { AuthApi } from '../../api/apis/AuthApi';
import { setAuthToken, setRefreshToken } from '../../utils/auth';

// Ensure the basePath includes the correct hostname and port.
const authApi = new AuthApi(
  new Configuration({
    basePath: import.meta.env.VITE_API_BASE_URL,
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
    const credential = credentialResponse.credential ?? '';
    const decoded: GooglePayload = jwtDecode(credential);

    try {
      // IMPORTANT: Call the auth endpoint by passing an object that
      // has a property named "authorization" whose value is the Google ID token,
      // prefixed with "Bearer ". This is what the generated API client expects.
      const authResponse = await authApi.apiAuthGooglePost({
        authorization: `Bearer ${credential}`,
      });

      // --- Store both tokens ---
      const jwt = authResponse.accessToken;
      const refresh = authResponse.refreshToken;
      if (jwt) {
        setAuthToken(jwt);
      }
      if (refresh) {
        setRefreshToken(refresh);
      }

      // Update React auth context with the user details.
      setUser({
        id: decoded.sub,
        name: decoded.name,
        email: decoded.email,
        picture: decoded.picture,
        idToken: jwt,
      });
    } catch {
      handleLoginError();
    }
  };

  const handleLoginError = () => {
    // Clear any auth data
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