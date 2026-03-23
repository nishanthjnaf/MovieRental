import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';

@Injectable({ providedIn: 'root' })
export class WatchlistService {
  private baseUrl = 'http://localhost:5287/api/Watchlist';

  constructor(private http: HttpClient) {}

  add(userId: number, movieId: number) {
    return this.http.post(this.baseUrl, { userId, movieId });
  }

  getByUser(userId: number) {
    return this.http.get<any[]>(`${this.baseUrl}/user/${userId}`);
  }

  remove(id: number) {
    return this.http.delete(`${this.baseUrl}/${id}`);
  }
}

