// src/app/services/auth.service.ts

import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { BehaviorSubject, Observable } from 'rxjs';
import { RegisterRequest } from '../models/register-request';
import { LoginRequest } from '../models/login-request';

@Injectable({
  providedIn: 'root'
})
export class AuthService {
  private BASE_URL = 'https://localhost:7152/api/auth';
private loginStateSubject = new BehaviorSubject<boolean>(this.hasValidToken());
  public loginState$ = this.loginStateSubject.asObservable();
  router: any;
  constructor(private http: HttpClient) {}

 login(dto: LoginRequest): Observable<{ token: string }> {
  return this.http.post<{ token: string }>(`${this.BASE_URL}/login`, dto);
}


  register(data: RegisterRequest): Observable<void> {
    return this.http.post<void>(`${this.BASE_URL}/register`, data);
  }

  getCurrentUser(): Observable<any> {
    return this.http.get(`${this.BASE_URL}/me`);
  }

  logout(): void {
    localStorage.removeItem('token');
      this.loginStateSubject.next(false);
    window.location.href = '/login';
  }

  getToken(): string | null {
    return localStorage.getItem('token');
  }

  isLoggedIn(): boolean {
    return !!this.getToken();
  }
    setToken(token: string): void {
    localStorage.setItem('token', token);
    this.loginStateSubject.next(true);
  }

  /*private hasToken(): boolean {
    return !!localStorage.getItem('token');
  }*/
  private hasValidToken(): boolean {
    const token = localStorage.getItem('token');
    if (!token) return false;

    try {
      const payload = JSON.parse(atob(token.split('.')[1]));
      const now = Math.floor(Date.now() / 1000);
      return payload.exp > now;
    } catch {
      return false;
    }
  }

  forgotPassword(email: string): Observable<void> {
  return this.http.post<void>(`${this.BASE_URL}/forgot-password`, { email });
}
resetPassword(token:string,newPassword:string): Observable<void>{
  return this.http.post<void>(`${this.BASE_URL}/reset-password`, { token,newPassword });

}
confirmEmail(token: string): Observable<any> {
  return this.http.get(`${this.BASE_URL}/confirm-email?token=${token}`, { responseType: 'text' });
}

}
