import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';

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
}