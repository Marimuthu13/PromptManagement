export interface User {
  id: string;
  name: string;
  email: string;
}

export interface AuthResponse {
  token: string;
  user: User;
}

export interface LoginRequest {
  email: string;
  password?: string; // Optional if we just want a simple login for now based on email, but backend requires email and password usually.
}

export interface RegisterRequest {
  username: string;
  email: string;
  password?: string;
}
