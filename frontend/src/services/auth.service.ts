import { Injectable, computed, inject, signal } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable, tap } from 'rxjs';

import { AuthResponse, AuthUser, LoginRequest, SignUpRequest } from '../DTOs/auth.dto';

interface StoredSession {
  token: string;
  user: AuthUser;
}

@Injectable({
  providedIn: 'root'
})
export class AuthService {
  private readonly http = inject(HttpClient);
  private readonly storageKey = 'event-planner.auth.session';
  private readonly storedSession = this.readStoredSession();
  private readonly authToken = signal<string | null>(this.storedSession?.token ?? null);
  private readonly authUser = signal<AuthUser | null>(this.storedSession?.user ?? null);

  public readonly token = this.authToken.asReadonly();
  public readonly currentUser = this.authUser.asReadonly();
  public readonly isAuthenticated = computed(() => !!this.authToken() && !!this.authUser());

  public login(payload: LoginRequest): Observable<AuthResponse> {
    return this.http
      .post<AuthResponse>('/api/auth/login', payload)
      .pipe(tap((response) => this.storeSession(response)));
  }

  public signUp(payload: SignUpRequest): Observable<AuthResponse> {
    return this.http
      .post<AuthResponse>('/api/auth/signup', payload)
      .pipe(tap((response) => this.storeSession(response)));
  }

  public logout(): void {
    this.authToken.set(null);
    this.authUser.set(null);
    localStorage.removeItem(this.storageKey);
  }

  private storeSession(response: AuthResponse): void {
    this.authToken.set(response.token);
    this.authUser.set(response.user);
    localStorage.setItem(
      this.storageKey,
      JSON.stringify({
        token: response.token,
        user: response.user
      } satisfies StoredSession)
    );
  }

  private readStoredSession(): StoredSession | null {
    try {
      const value = localStorage.getItem(this.storageKey);
      return value ? (JSON.parse(value) as StoredSession) : null;
    } catch {
      localStorage.removeItem(this.storageKey);
      return null;
    }
  }
}
