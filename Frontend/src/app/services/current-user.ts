import { Injectable } from '@angular/core';
import { BehaviorSubject, catchError, Observable, of, tap, timeout } from 'rxjs';
import { jwtDecode } from 'jwt-decode';
import { UserService } from './user';

@Injectable({ providedIn: 'root' })
export class CurrentUserService {
  private userSubject = new BehaviorSubject<any | null>(null);
  user$ = this.userSubject.asObservable();

  constructor(private userService: UserService) {}

  get token(): string | null {
    return localStorage.getItem('token') || sessionStorage.getItem('token');
  }

  get decodedUsername(): string | null {
    const token = this.token;
    if (!token) return null;
    const decoded: any = jwtDecode(token);
    return decoded?.unique_name || decoded?.name || decoded?.['http://schemas.xmlsoap.org/ws/2005/05/identity/claims/name'] || null;
  }

  get decodedUserId(): number {
    const token = this.token;
    if (!token) return 0;
    const decoded: any = jwtDecode(token);
    const raw =
      decoded?.nameid ||
      decoded?.sub ||
      decoded?.userId ||
      decoded?.['http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier'];
    const id = Number(raw);
    return Number.isFinite(id) && id > 0 ? id : 0;
  }

  loadCurrentUser(): Observable<any | null> {
    const username = this.decodedUsername;
    const fallbackId = this.decodedUserId;
    if (!username) {
      if (fallbackId > 0) {
        const fallback = { id: fallbackId };
        this.userSubject.next(fallback);
        return of(fallback);
      }
      return of(null);
    }

    if (this.userSubject.value?.username === username) {
      return of(this.userSubject.value);
    }

    return this.userService.getByUsername(username).pipe(
      timeout(7000),
      tap((user) => this.userSubject.next(user)),
      catchError(() => {
        if (fallbackId > 0) {
          const fallback = { id: fallbackId, username };
          this.userSubject.next(fallback);
          return of(fallback);
        }
        return of(null);
      })
    );
  }

  get currentUserId(): number {
    return this.userSubject.value?.id ?? this.decodedUserId ?? 0;
  }

  clear() {
    this.userSubject.next(null);
  }
}

