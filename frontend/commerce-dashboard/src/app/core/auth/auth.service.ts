import { Injectable, signal } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable, tap, finalize, shareReplay } from 'rxjs';
import { LoginRequest, LoginResponse, RegisterRequest } from '../models/auth.models';

@Injectable({
  providedIn: 'root'
})
export class AuthService {
  private readonly apiUrl =
    'http://localhost:5170/api/auth';

  private readonly accessTokenSignal =
    signal<string | null>(null);

  readonly isAuthenticated = signal(false);

  private refreshRequest: Observable<LoginResponse> | null = null;

  constructor(private readonly http: HttpClient) {
  }

  register(request: RegisterRequest): Observable<void> {
    return this.http.post<void>(
      `${this.apiUrl}/register`,
      request
    );
  }

  login(request: LoginRequest): Observable<LoginResponse> {
    return this.http.post<LoginResponse>(
      `${this.apiUrl}/login`,
      request,
      {
        withCredentials: true
      }
    ).pipe(
      tap(response => {
        this.setSession(response);
      })
    );
  }

refresh(): Observable<LoginResponse> {
  if (this.refreshRequest != null) {
    return this.refreshRequest;
  }

  this.refreshRequest = this.http.post<LoginResponse>(
    `${this.apiUrl}/refresh`,
    {},
    {
      withCredentials: true
    }
  ).pipe(
    tap(response => {
      this.setSession(response);
    }),

    finalize(() => {
      this.refreshRequest = null;
    }),

    shareReplay(1)
  );

  return this.refreshRequest;
}

  logout(): Observable<void> {
    return this.http.post<void>(
      `${this.apiUrl}/logout`,
      {},
      {
        withCredentials: true
      }
    ).pipe(
      finalize(() => {
        this.clearSession();
      })
    );
  }

  getAccessToken(): string | null {
    return this.accessTokenSignal();
  }

  private setSession(response: LoginResponse): void {
    this.accessTokenSignal.set(response.accessToken);
    this.isAuthenticated.set(true);
  }

  clearSession(): void {
    this.accessTokenSignal.set(null);
    this.isAuthenticated.set(false);
  }
}


/*
Refresh: 
Interceptor
"Refresh my authentication"
       ↓
AuthService
"I'll make sure only one refresh happens"
*/