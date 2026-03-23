import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';

@Injectable({ providedIn: 'root' })
export class UserService {
  private baseUrl = 'http://localhost:5287/api/User';

  constructor(private http: HttpClient) {}

  getById(id: number) {
    return this.http.get<any>(`${this.baseUrl}/${id}`);
  }

  getByUsername(username: string) {
    return this.http.get<any>(`${this.baseUrl}/by-username/${encodeURIComponent(username)}`);
  }

  getRentedMovies(userId: number) {
    return this.http.get<any[]>(`${this.baseUrl}/${userId}/rented-movies`);
  }

  getAll() {
    return this.http.get<any[]>(this.baseUrl);
  }

  updateUser(id: number, data: any) {
    return this.http.put<any>(`${this.baseUrl}/${id}`, data);
  }

  resetPassword(id: number, payload: { oldPassword: string; newPassword: string; confirmPassword: string }) {
    return this.http.patch(
      `${this.baseUrl}/${id}/reset-password`,
      payload,
      { responseType: 'text' as 'json' }
    );
  }

  deleteUser(id: number) {
    return this.http.delete(`${this.baseUrl}/${id}`);
  }
}

