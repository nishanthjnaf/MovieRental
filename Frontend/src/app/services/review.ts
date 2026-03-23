import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { catchError, of, throwError } from 'rxjs';

@Injectable({ providedIn: 'root' })
export class ReviewService {
  private baseUrl = 'http://localhost:5287/api/Review';

  constructor(private http: HttpClient) {}

  addReview(data: any) {
    return this.http.post(this.baseUrl, data);
  }

  getByMovie(movieId: number) {
    return this.http.get<any[]>(`${this.baseUrl}/movie/${movieId}`);
  }

  getByUser(userId: number) {
    return this.http.get<any[]>(`${this.baseUrl}/user/${userId}`).pipe(
      catchError((err) => err?.status === 404 ? of([]) : throwError(() => err))
    );
  }

  deleteReview(id: number) {
    return this.http.delete(`${this.baseUrl}/${id}`, { responseType: 'text' });
  }
}

