import { ChangeDetectorRef, Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { CartStateService } from '../services/cart-state';
import { CurrentUserService } from '../services/current-user';
import { RentalService } from '../services/rental';
import { PaymentService } from '../services/payment';
import { ToastrService } from 'ngx-toastr';
import { Router } from '@angular/router';
import { catchError, finalize, of, switchMap } from 'rxjs';
import { InventoryService } from '../services/inventory';

@Component({
  selector: 'app-customer-cart',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './customer-cart.html'
})
export class CustomerCart {
  items: any[] = [];
  loading = false;
  showPaymentPopup = false;
  selectedPaymentMethod = 1;

  constructor(
    private cart: CartStateService,
    private currentUser: CurrentUserService,
    private rentalService: RentalService,
    private paymentService: PaymentService,
    private toastr: ToastrService,
    private router: Router,
    private inventoryService: InventoryService,
    private cdr: ChangeDetectorRef
  ) {
    this.cart.reload();
    this.cart.cart$.subscribe((items) => {
      this.items = items;
      this.ensurePricesLoaded();
    });
  }

  priceByMovieId: Record<number, number> = {};
  private pricesLoading = new Set<number>();

  get total(): number {
    return this.items.reduce((sum, m) => {
      const days = Math.max(1, Number(m?.rentalDays || 1));
      const price = Number(this.priceByMovieId[m.id] || 0);
      return sum + (price * days);
    }, 0);
  }

  private ensurePricesLoaded() {
    (this.items || []).forEach((m) => {
      const movieId = Number(m?.id);
      if (!movieId) return;
      if (Number.isFinite(this.priceByMovieId[movieId]) || this.pricesLoading.has(movieId)) return;
      const fallbackPrice = Number(m?.rentalPrice ?? m?.pricePerDay);
      if (Number.isFinite(fallbackPrice) && fallbackPrice > 0) {
        this.priceByMovieId[movieId] = fallbackPrice;
      }
      this.pricesLoading.add(movieId);
      this.inventoryService.getByMovie(movieId).pipe(catchError(() => of([]))).subscribe((res: any) => {
        const rows = Array.isArray(res)
          ? res
          : (Array.isArray(res?.data) ? res.data : (res ? [res] : []));
        const first = rows.find((r: any) => r?.isAvailable) || rows[0];
        const price = Number(first?.rentalPrice);
        if (Number.isFinite(price)) this.priceByMovieId[movieId] = price;
        this.pricesLoading.delete(movieId);
        this.cdr.detectChanges();
      });
    });
  }

  remove(movieId: number) {
    this.cart.remove(movieId);
  }

  setDays(movieId: number, days: number) {
    this.cart.updateRentalDays(movieId, days);
  }

  openCheckout() {
    const userId = this.currentUser.currentUserId;
    if (!userId || this.items.length === 0) return;
    this.showPaymentPopup = true;
  }

  checkout() {
    const userId = this.currentUser.currentUserId || this.currentUser.decodedUserId;
    if (!userId || this.items.length === 0) return;
    this.loading = true;

    const movieIds = this.items.map((m) => m.id);
    const rentalDaysPerMovie = this.items.map((m) => Math.max(1, Number(m?.rentalDays || 1)));

    this.rentalService.createRental({
      userId,
      movieIds,
      rentalDays: rentalDaysPerMovie[0],
      rentalDaysPerMovie
    }).pipe(
      switchMap((rental: any) =>
        this.paymentService.makePayment({
          rentalId: rental.id,
          method: this.selectedPaymentMethod,
          isSuccess: true
        })
      ),
      catchError(() => of(null)),
      finalize(() => (this.loading = false))
    ).subscribe({
      next: (res) => {
        if (!res) {
          this.toastr.error('Payment failed');
          return;
        }
        this.toastr.success(`Rental successful for ${movieIds.length} movie(s)`);
        this.showPaymentPopup = false;
        this.cart.clear();
        this.router.navigate(['/dashboard/rentals']);
      },
      error: () => this.toastr.error('Unable to complete checkout')
    });
  }

  goToMovies() {
    this.router.navigate(['/dashboard/movies']);
  }

  openMovie(movieId: number) {
    this.router.navigate(['/dashboard/movie', movieId]);
  }
}

