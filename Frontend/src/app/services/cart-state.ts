import { Injectable } from '@angular/core';
import { BehaviorSubject } from 'rxjs';
import { jwtDecode } from 'jwt-decode';

@Injectable({ providedIn: 'root' })
export class CartStateService {
  private cartSubject = new BehaviorSubject<any[]>(this.readCart());
  cart$ = this.cartSubject.asObservable();

  private get storageKey(): string {
    try {
      const token = localStorage.getItem('token') || sessionStorage.getItem('token');
      if (!token) return 'movie_rental_cart_guest';
      const decoded: any = jwtDecode(token);
      const id = decoded?.nameid || decoded?.sub || decoded?.userId ||
        decoded?.['http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier'];
      return id ? `movie_rental_cart_${id}` : 'movie_rental_cart_guest';
    } catch {
      return 'movie_rental_cart_guest';
    }
  }

  private readCart(): any[] {
    try {
      const raw = localStorage.getItem(this.storageKey);
      return raw ? JSON.parse(raw) : [];
    } catch {
      return [];
    }
  }

  private persist(items: any[]) {
    localStorage.setItem(this.storageKey, JSON.stringify(items));
    this.cartSubject.next(items);
  }

  get items(): any[] {
    return this.cartSubject.value;
  }

  // Call this after login to reload the correct user's cart
  reload() {
    this.cartSubject.next(this.readCart());
  }

  add(movie: any) {
    const exists = this.items.some((m) => m.id === movie.id);
    if (exists) return;
    this.persist([...this.items, { ...movie, rentalDays: 7 }]);
  }

  updateRentalDays(movieId: number, rentalDays: number) {
    const days = Math.max(1, Math.min(30, Number(rentalDays || 1)));
    this.persist(
      this.items.map((m) => (m.id === movieId ? { ...m, rentalDays: days } : m))
    );
  }

  remove(movieId: number) {
    this.persist(this.items.filter((m) => m.id !== movieId));
  }

  clear() {
    this.persist([]);
  }
}

