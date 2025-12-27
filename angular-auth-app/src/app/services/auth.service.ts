import { HttpClient } from '@angular/common/http';
import { inject, Injectable } from '@angular/core';
import { Router } from '@angular/router';
import { BehaviorSubject, Observable } from 'rxjs';
import { AuthResponse } from 'src/app/models/auth/authResponse';
import { UserInfo } from 'src/app/models/user/userInfo';

@Injectable({
  providedIn: 'root'
})
export class AuthService {

  private apiUrl = 'https://localhost:7146/api/Auth';
  private currentUserSubject = new BehaviorSubject<UserInfo | null>(null);
  private tokenSubject = new BehaviorSubject<string | null>(null);

  public currentUser$ = this.currentUserSubject.asObservable();
  public token$ = this.tokenSubject.asObservable();

  private http = inject(HttpClient);
  private router = inject(Router);

  constructor() { }

  private loadStoreAuth(): void {
    const token = sessionStorage.getItem('accessToken');
    const user = sessionStorage.getItem('user');

    if(token && user){
      this.tokenSubject.next(token);
      this.currentUserSubject.next(JSON.parse(user));
    }
  }

  getLoginUrl(): Observable<{loginUrl: string, state: string}> {
    return this.http.get<{loginUrl: string, state: string}>(`${this.apiUrl}/login-url`);
  }

  login(): void {
      this.getLoginUrl().subscribe(response => {
        sessionStorage.setItem('authState', response.state);
        window.location.href = response.loginUrl;
      });
  }

  handleCallback(code: string, state: string): Observable<AuthResponse>{
    const storedState = sessionStorage.getItem('authState');
    if(state !== storedState){
      throw new Error('Invalid state parameter');
    }

    return this.http.post<AuthResponse>(`${this.apiUrl}/callback`, {code, state});
  }

  procassAuthResponse(authResponse: AuthResponse): void {
    sessionStorage.setItem('accessToken', authResponse.accessToken);
    sessionStorage.setItem('idToken', authResponse.idToken);
    sessionStorage.setItem('refreshToken', authResponse.refreshToken);
    sessionStorage.setItem('user', JSON.stringify(authResponse.user));
    sessionStorage.setItem('tokenExpiry', (Date.now() + (authResponse.expiresIn * 1000)).toString());
    sessionStorage.removeItem('authState');

    this.tokenSubject.next(authResponse.accessToken);
    this.currentUserSubject.next(authResponse.user);
  }

  refreshToken(): Observable<AuthResponse> {
    const refreshToken = sessionStorage.getItem('refreshToken');
    if(!refreshToken){
      throw new Error('No refresh token available');
    }

    return this.http.post<AuthResponse>(`${this.apiUrl}/refresh`, { refreshToken });
  }

  logout(): void {
    sessionStorage.clear();
    this.currentUserSubject.next(null);
    this.tokenSubject.next(null);
    this.router.navigate(['/']);
  }

  isAuthenticated(): boolean {
    const token = sessionStorage.getItem('accessToken');
    const expiry = sessionStorage.getItem('tokenExpiry');

    if(!token || !expiry)
      return false;

    return Date.now() < parseInt(expiry);
  }

  getToken(): string | null {
    return this.isAuthenticated() ? sessionStorage.getItem('accessToken') : null;
  }

  getCurrentUser(): UserInfo | null {
    return this.currentUserSubject.value;
  }

}
