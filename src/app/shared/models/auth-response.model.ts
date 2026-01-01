import { User } from './user.model';

export interface AuthResponse {
  id: number;
  accessToken: string;
  refreshToken: string;
  firstName: string;
  lastName: string;
  email: string;
  role: string;
}

export interface LoginRequest {
  email: string;
  password: string;
}

export interface RegisterRequest {
  firstName: string;
  lastName: string;
  email: string;
  password: string;
  confirmPassword: string;
}
