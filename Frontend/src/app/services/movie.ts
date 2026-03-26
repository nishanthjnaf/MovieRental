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

  filter(params: {
    searchTerm?: string;
    genreIds?: number[];
    languages?: string[];
    minYear?: number;
    maxYear?: number;
    minPrice?: number;
    maxPrice?: number;
  }) {
    const p = new URLSearchParams();
    if (params.searchTerm) p.set('searchTerm', params.searchTerm);
    (params.genreIds || []).forEach(id => p.append('genreIds', String(id)));
    (params.languages || []).forEach(l => p.append('languages', l));
    if (params.minYear) p.set('minYear', String(params.minYear));
    if (params.maxYear) p.set('maxYear', String(params.maxYear));
    if (params.minPrice) p.set('minPrice', String(params.minPrice));
    if (params.maxPrice) p.set('maxPrice', String(params.maxPrice));
    return this.http.get<any[]>(`${this.baseUrl}/filter?${p.toString()}`);
  }

  getSuggestions(userId: number) {
    return this.http.get<any[]>(`${this.baseUrl}/suggestions/${userId}`);
  }
}