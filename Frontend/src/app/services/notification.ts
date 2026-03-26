import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { BehaviorSubject, catchError, of } from 'rxjs';

@Injectable({ providedIn: 'root' })
export class NotificationService {
  private baseUrl = 'http://localhost:5287/api/Notification';
  private unreadSubject = new BehaviorSubject<number>(0);
  unread$ = this.unreadSubject.asObservable();

  constructor(private http: HttpClient) {}

  getForUser(userId: number) {
    return this.http.get<any[]>(`${this.baseUrl}/user/${userId}`);
  }

  refreshUnread(userId: number) {
    this.http.get<number>(`${this.baseUrl}/user/${userId}/unread-count`)
      .pipe(catchError(() => of(0)))
      .subscribe(count => this.unreadSubject.next(count));
  }

  markRead(id: number) {
    return this.http.patch(`${this.baseUrl}/${id}/read`, {}, { responseType: 'text' });
  }

  markAllRead(userId: number) {
    return this.http.patch(`${this.baseUrl}/user/${userId}/read-all`, {}, { responseType: 'text' });
  }

  delete(id: number) {
    return this.http.delete(`${this.baseUrl}/${id}`, { responseType: 'text' });
  }

  broadcast(sentByUserId: number, title: string, message: string) {
    return this.http.post<any>(`${this.baseUrl}/admin/broadcast`, { sentByUserId, title, message });
  }

  getBroadcasts() {
    return this.http.get<any[]>(`${this.baseUrl}/admin/broadcasts`);
  }

  deleteBroadcast(id: number) {
    return this.http.delete(`${this.baseUrl}/admin/broadcasts/${id}`, { responseType: 'text' });
  }

  checkExpiry() {
    return this.http.post(`${this.baseUrl}/check-expiry`, {}, { responseType: 'text' });
  }

  // Push rate-movie notification (called from watch page)
  pushRateMovie(userId: number, movieId: number, movieTitle: string) {
    // We call a generic push via a dedicated endpoint
    return this.http.post(`${this.baseUrl}/push`, {
      userId, type: 'rate_movie',
      title: 'Enjoyed the movie?',
      message: `Share your thoughts on "${movieTitle}" — leave a rating!`,
      relatedId: movieId
    }, { responseType: 'text' }).pipe(catchError(() => of(null)));
  }
}
