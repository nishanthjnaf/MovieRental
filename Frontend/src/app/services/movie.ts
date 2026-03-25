import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';

@Injectable({ providedIn: 'root' })
export class MovieService {

  baseUrl = 'http://localhost:5287/api/Movie';

  constructor(private http: HttpClient) {}

  getAll() {
    return this.http.get<any[]>(this.baseUrl);
  }

  getById(id: number) {
    return this.http.get<any>(`${this.baseUrl}/${id}`);
  }

  add(data: any) {
    return this.http.post(this.baseUrl, data);
  }

  update(id: number, data: any) {
    return this.http.put(`${this.baseUrl}/${id}`, data);
  }

  delete(id: number) {
    return this.http.delete(`${this.baseUrl}/${id}`);
  }

  search(name: string, pageNumber: number = 1, pageSize: number = 24) {
    return this.http.get<any>(
      `${this.baseUrl}/search?searchTerm=${encodeURIComponent(name)}&pageNumber=${pageNumber}&pageSize=${pageSize}`
    );
  }

  // ===================== TOP USER RATED =====================
  getTopUserRated(count: number = 10) {
    return this.http.get<any[]>(
      `${this.baseUrl}/top-user-rated?count=${count}`
    );
  }

  getTopRented(count: number = 10) {
    return this.http.get<any[]>(
      `${this.baseUrl}/top-rented?count=${count}`
    );
  }

  getSuggestions(userId: number) {
    return this.http.get<any[]>(`${this.baseUrl}/suggestions/${userId}`);
  }
}