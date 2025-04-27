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