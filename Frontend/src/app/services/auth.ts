import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { jwtDecode } from 'jwt-decode';

@Injectable({ providedIn: 'root' })
export class AuthService {

  private baseUrl = 'http://localhost:5287/api/Authentication';

  constructor(private http: HttpClient) {}

  register(data: any) {
    return this.http.post(`${this.baseUrl}/Register`, data);
  }

  login(data: any) {
    return this.http.post(`${this.baseUrl}/Login`, data);
  }

  getToken(): string | null {
    return localStorage.getItem('token') || sessionStorage.getItem('token');
  }

  isTokenValid(): boolean {
    const token = this.getToken();
    if (!token) return false;

    try {
      const decoded: any = jwtDecode(token);
      // exp is in seconds, Date.now() is in milliseconds
      const isExpired = decoded.exp * 1000 < Date.now();
      return !isExpired;
    } catch {
      // token is malformed / tampered
      return false;
    }
  }

  clearSession(): void {
    localStorage.removeItem('token');
    localStorage.removeItem('role');
    sessionStorage.removeItem('token');
  }
}