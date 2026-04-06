import { Injectable } from '@angular/core';
import { BehaviorSubject, catchError, of, switchMap, tap } from 'rxjs';
import { HttpClient } from '@angular/common/http';
import { CurrentUserService } from './current-user';

@Injectable({ providedIn: 'root' })
export class CartStateService {
  private readonly baseUrl = 'http://localhost:5287/api/Cart';
  private cartSubject = new BehaviorSubject<any[]>([]);
  cart$ = this.cartSubject.asObservable();

  constructor(private http: HttpClient, private currentUser: CurrentUserService) {}

  private getUserId(): number {
    const fromSubject = this.currentUser.currentUserId;
    if (fromSubject > 0) return fromSubject;
    return this.currentUser.decodedUserId;
  }

  get items(): any[] { return this.cartSubject.value; }

  reload() {
    this.currentUser.loadCurrentUser().subscribe(() => {
      const uid = this.getUserId();
      if (!uid) { this.cartSubject.next([]); return; }
      this.http.get<any[]>(`${this.baseUrl}/${uid}`)
        .pipe(catchError(() => of([])))
        .subscribe(items => this.cartSubject.next(items || []));
    });
  }

  reset() { this.cartSubject.next([]); }

  // Add a movie to cart
  add(movie: any) {
    const movieId = movie?.id ?? movie?.movieId;
    if (!movieId) return of(null);

    return this.currentUser.loadCurrentUser().pipe(
      switchMap(() => {
        const uid = this.getUserId();
        if (!uid) return of(null);
        const exists = this.items.some(m => !m.isSeries && (m.movieId ?? m.id) === movieId);
        if (exists) return of('exists');
        return this.http.post(`${this.baseUrl}/${uid}/add`, { movieId, rentalDays: 7 }, { responseType: 'text' })
          .pipe(tap(() => this.fetchCart(uid)), catchError(() => of(null)));
      })
    );
  }

  // Add a series to cart
  addSeries(series: any) {
    const seriesId = series?.id ?? series?.seriesId;
    if (!seriesId) return of(null);

    return this.currentUser.loadCurrentUser().pipe(
      switchMap(() => {
        const uid = this.getUserId();
        if (!uid) return of(null);
        const exists = this.items.some(m => m.isSeries && m.seriesId === seriesId);
        if (exists) return of('exists');
        return this.http.post(`${this.baseUrl}/${uid}/add-series`, { seriesId, rentalDays: 7 }, { responseType: 'text' })
          .pipe(tap(() => this.fetchCart(uid)), catchError(() => of(null)));
      })
    );
  }

  private fetchCart(uid: number) {
    this.http.get<any[]>(`${this.baseUrl}/${uid}`)
      .pipe(catchError(() => of([])))
      .subscribe(items => this.cartSubject.next(items || []));
  }

  updateRentalDays(movieId: number, rentalDays: number) {
    const uid = this.getUserId();
    if (!uid) return;
    const days = Math.max(1, Math.min(30, Number(rentalDays) || 1));
    this.cartSubject.next(this.items.map(m => (m.movieId === movieId ? { ...m, rentalDays: days } : m)));
    this.http.patch(`${this.baseUrl}/${uid}/days`, { movieId, rentalDays: days }, { responseType: 'text' })
      .pipe(catchError(() => of(null))).subscribe();
  }

  updateSeriesRentalDays(seriesId: number, rentalDays: number) {
    const uid = this.getUserId();
    if (!uid) return;
    const days = Math.max(1, Math.min(30, Number(rentalDays) || 1));
    this.cartSubject.next(this.items.map(m => (m.isSeries && m.seriesId === seriesId ? { ...m, rentalDays: days } : m)));
    this.http.patch(`${this.baseUrl}/${uid}/series-days`, { seriesId, rentalDays: days }, { responseType: 'text' })
      .pipe(catchError(() => of(null))).subscribe();
  }

  remove(movieId: number) {
    const uid = this.getUserId();
    if (!uid) return;
    this.cartSubject.next(this.items.filter(m => m.movieId !== movieId));
    this.http.delete(`${this.baseUrl}/${uid}/remove/${movieId}`, { responseType: 'text' })
      .pipe(catchError(() => of(null))).subscribe();
  }

  removeSeries(seriesId: number) {
    const uid = this.getUserId();
    if (!uid) return;
    this.cartSubject.next(this.items.filter(m => !(m.isSeries && m.seriesId === seriesId)));
    this.http.delete(`${this.baseUrl}/${uid}/remove-series/${seriesId}`, { responseType: 'text' })
      .pipe(catchError(() => of(null))).subscribe();
  }

  clear() {
    const uid = this.getUserId();
    if (!uid) return;
    this.cartSubject.next([]);
    this.http.delete(`${this.baseUrl}/${uid}/clear`, { responseType: 'text' })
      .pipe(catchError(() => of(null))).subscribe();
  }
}
