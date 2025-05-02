// src/utils/auth.ts
export function getAuthToken(): string | null {
  return localStorage.getItem('authToken');
}
  
export function setAuthToken(token: string) {
  localStorage.setItem('authToken', token);
}
  
export function clearAuthToken() {
  localStorage.removeItem('authToken');
}

export function setRefreshToken(token: string) {
  localStorage.setItem('refreshToken', token);
}

export function getRefreshToken(): string | null {
  return localStorage.getItem('refreshToken');
}

export function clearRefreshToken() {
  localStorage.removeItem('refreshToken');
}