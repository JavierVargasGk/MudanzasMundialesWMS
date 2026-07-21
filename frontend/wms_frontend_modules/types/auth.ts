// src/types/auth.ts

// types/auth.ts
export type UserRole = 'Administrador' | 'Analista' | 'Operario';

export interface UserSession {
  userId: string;
  email: string;
  fullName: string;
  role: UserRole;
  clientId?: string;
  clientName?: string;
}

export interface AuthErrorResponse {
  message: string;
  code?: string;
  isLockedOut?: boolean;
  lockoutEndUtc?: string;
  remainingAttempts?: number;
  isInactive?: boolean;
}

export interface LoginResponse {
  accessToken: string;
  refreshToken: string;
  accessTokenExpiresAtUtc: string;
  userId: number;
  role: string;
  nombreCompleto: string;
}