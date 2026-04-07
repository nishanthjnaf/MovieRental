import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable, catchError, of } from 'rxjs';

@Injectable({ providedIn: 'root' })
export class SeriesService {
  private baseUrl = 'http://localhost:5287/api/Series';
  private rentalUrl = 'http://localhost:5287/api/SeriesRental';
  private watchlistUrl = 'http://localhost:5287/api/SeriesWatchlist';
  private reviewUrl = 'http://localhost:5287/api/SeasonReview';

  constructor(private http: HttpClient) {}

  // ── Series CRUD ──
  getAll(): Observable<any[]> {
    return this.http.get<any[]>(this.baseUrl);
  }

  getById(id: number): Observable<any> {
    return this.http.get<any>(`${this.baseUrl}/${id}`);
  }

  add(data: any): Observable<any> {
    return this.http.post(this.baseUrl, data);
  }

  update(id: number, data: any): Observable<any> {
    return this.http.put(`${this.baseUrl}/${id}`, data);
  }

  delete(id: number): Observable<any> {
    return this.http.delete(`${this.baseUrl}/${id}`);
  }

  getNew(count = 10): Observable<any[]> {
    return this.http.get<any[]>(`${this.baseUrl}/new?count=${count}`);
  }

  getTopRated(count = 10): Observable<any[]> {
    return this.http.get<any[]>(`${this.baseUrl}/top-rated?count=${count}`);
  }

  getTopRented(count = 10): Observable<any[]> {
    return this.http.get<any[]>(`${this.baseUrl}/top-rented?count=${count}`);
  }

  getSuggestions(userId: number): Observable<any[]> {
    return this.http.get<any[]>(`${this.baseUrl}/suggestions/${userId}`);
  }

  // ── Rental ──
  createRental(data: { userId: number; seriesId: number; rentalDays: number }): Observable<any> {
    return this.http.post(this.rentalUrl, data);
  }

  getRentalsByUser(userId: number): Observable<any[]> {
    return this.http.get<any[]>(`${this.rentalUrl}/user/${userId}`);
  }

  endRental(id: number): Observable<any> {
    return this.http.patch(`${this.rentalUrl}/end/${id}`, {}, { responseType: 'text' });
  }

  renewRental(id: number, daysToAdd: number): Observable<any> {
    return this.http.patch(`${this.rentalUrl}/renew/${id}`, { daysToAdd });
  }

  // ── Watchlist ──
  addToWatchlist(userId: number, seriesId: number): Observable<any> {
    return this.http.post(this.watchlistUrl, { userId, seriesId });
  }

  getWatchlistByUser(userId: number): Observable<any[]> {
    return this.http.get<any[]>(`${this.watchlistUrl}/user/${userId}`);
  }

  removeFromWatchlist(id: number): Observable<any> {
    return this.http.delete(`${this.watchlistUrl}/${id}`, { responseType: 'text' });
  }

  addSeason(data: any): Observable<any> {
    return this.http.post(`${this.baseUrl}/season`, data);
  }

  addEpisode(data: any): Observable<any> {
    return this.http.post(`${this.baseUrl}/episode`, data);
  }

  // ── Season Reviews ──
  addSeasonReview(data: { userId: number; seasonId: number; rating: number; comment: string }): Observable<any> {
    return this.http.post(this.reviewUrl, data);
  }

  getReviewsBySeason(seasonId: number): Observable<any[]> {
    return this.http.get<any[]>(`${this.reviewUrl}/season/${seasonId}`);
  }

  getReviewsByUser(userId: number): Observable<any[]> {
    return this.http.get<any[]>(`${this.reviewUrl}/user/${userId}`).pipe(
      catchError(err => err?.status === 404 ? of([]) : of([]))
    );
  }

  deleteSeasonReview(id: number): Observable<any> {
    return this.http.delete(`${this.reviewUrl}/${id}`, { responseType: 'text' });
  }
}
