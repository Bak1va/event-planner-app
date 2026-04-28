export interface AuthUser {
  id: number;
  name: string;
  firstName: string;
  lastName: string;
  phoneNumber: string;
  email: string;
  dateAdded: string;
  dateModified: string;
}

export interface LoginRequest {
  email: string;
  password: string;
}

export interface SignUpRequest {
  email: string;
  password: string;
  firstName: string;
  lastName: string;
  phoneNumber: string;
}

export interface AuthResponse {
  token: string;
  user: AuthUser;
}
