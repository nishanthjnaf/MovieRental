import { ChangeDetectorRef, Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { CartStateService } from '../services/cart-state';
import { CurrentUserService } from '../services/current-user';
import { RentalService } from '../services/rental';
import { PaymentService } from '../services/payment';
import { ToastrService } from 'ngx-toastr';
import { Router } from '@angular/router';
import { catchError, finalize, of } from 'rxjs';
import { InventoryService } from '../services/inventory';

interface Promo {
  code: string;
  label: string;
  description: string;
  minItems: number;
  discountPct: number;
}

@Component({
  selector: 'app-customer-cart',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './customer-cart.html'
})
export class CustomerCart implements OnInit {
  items: any[] = [];
  loading = false;
  showPaymentPopup = false;
  selectedPaymentMethod = 1;

  appliedPromo: Promo | null = null;
  promos: Promo[] = [];

  constructor(
    private cart: CartStateService,
    private currentUser: CurrentUserService,
    private rentalService: RentalService,
    private paymentService: PaymentService,
    private toastr: ToastrService,
    private router: Router,
    private inventoryService: InventoryService,
    private cdr: ChangeDetectorRef
  ) {}

  ngOnInit() {
    this.loadPromos();
    this.cart.reload();
    this.cart.cart$.subscribe((items) => {
      this.items = items;
      this.ensurePricesLoaded();
      this.autoApplyPromo();
      this.cdr.detectChanges();
    });
  }

  private loadPromos() {
    this.paymentService.getPromos()
      .pipe(catchError(() => of([])))
      .subscribe((promos: any[]) => {
        this.promos = promos;
        this.autoApplyPromo();
        this.cdr.detectChanges();
      });
  }

  priceByMovieId: Record<number, number> = {};
  private pricesLoading = new Set<number>();

  // ── Promo helpers ──────────────────────────────────────────────────────────

  get eligiblePromo(): Promo | null {
    return [...this.promos].reverse().find(p => this.items.length >= p.minItems) ?? null;
  }

  isPromoEligible(promo: Promo): boolean {
    return this.items.length >= promo.minItems;
  }

  isPromoApplied(promo: Promo): boolean {
    return this.appliedPromo?.code === promo.code;
  }

  applyPromo(promo: Promo) {
    if (!this.isPromoEligible(promo)) {
      this.toastr.warning(`Add ${promo.minItems - this.items.length} more movie(s) to use this offer`);
      return;
    }
    if (this.isPromoApplied(promo)) {
      this.appliedPromo = null;
      this.toastr.info('Promo removed');
      this.cdr.detectChanges();
      return;
    }

    // Validate via backend
    this.paymentService.applyPromo(promo.code, this.items.length)
      .pipe(catchError(() => of(null)))
      .subscribe((res: any) => {
        if (res?.isValid) {
          this.appliedPromo = { ...promo, discountPct: res.discountPct };
          this.toastr.success(res.message || `${promo.label} applied!`);
        } else {
          this.toastr.warning(res?.message || 'Promo could not be applied');
        }
        this.cdr.detectChanges();
      });
  }

  private autoApplyPromo() {
    const best = this.eligiblePromo;
    if (best && (!this.appliedPromo || !this.isPromoEligible(this.appliedPromo))) {
      this.appliedPromo = best;
    } else if (!best) {
      this.appliedPromo = null;
    }
  }

  // ── Totals ─────────────────────────────────────────────────────────────────

  get subtotal(): number {
    return this.items.reduce((sum, m) => {
      const days = Math.max(3, Number(m?.rentalDays || 3));
      const price = Number(this.priceByMovieId[m.movieId] || 0);
      return sum + price * days;
    }, 0);
  }

  get discountAmount(): number {
    if (!this.appliedPromo) return 0;
    return this.subtotal * (this.appliedPromo.discountPct / 100);
  }

  get total(): number {
    return this.subtotal - this.discountAmount;
  }

  get hasInvalidDays(): boolean {
    return this.items.some(m => {
      const days = Number(m?.rentalDays);
      return !days || days < 3;
    });
  }

  // ── Upsell helpers ─────────────────────────────────────────────────────────

  get showUpsell5(): boolean { return this.items.length === 1; }
  get showUpsell10(): boolean { return this.items.length === 2; }

  // ── Prices ─────────────────────────────────────────────────────────────────

  private ensurePricesLoaded() {
    (this.items || []).forEach((m) => {
      const movieId = Number(m?.movieId);
      if (!movieId) return;
      if (Number.isFinite(this.priceByMovieId[movieId]) || this.pricesLoading.has(movieId)) return;
      this.pricesLoading.add(movieId);
      this.inventoryService.getByMovie(movieId).pipe(catchError(() => of([]))).subscribe((res: any) => {
        const rows = Array.isArray(res) ? res : (Array.isArray(res?.data) ? res.data : (res ? [res] : []));
        const first = rows.find((r: any) => r?.isAvailable) || rows[0];
        const price = Number(first?.rentalPrice);
        if (Number.isFinite(price)) this.priceByMovieId[movieId] = price;
        this.pricesLoading.delete(movieId);
        this.cdr.detectChanges();
      });
    });
  }

  // ── Actions ────────────────────────────────────────────────────────────────

  remove(movieId: number) { this.cart.remove(movieId); }

  setDays(movieId: number, days: number) {
    this.cart.updateRentalDays(movieId, Number(days) || 1);
  }

  openCheckout() {
    if (!this.currentUser.currentUserId || this.items.length === 0) return;
    this.showPaymentPopup = true;
  }

  checkout() {
    const userId = this.currentUser.currentUserId || this.currentUser.decodedUserId;
    if (!userId || this.items.length === 0) return;
    this.loading = true;

    const movieIds = this.items.map(m => m.movieId);
    const rentalDaysPerMovie = this.items.map(m => Math.max(3, Number(m?.rentalDays || 3)));

    this.rentalService.createRental({ userId, movieIds, rentalDays: rentalDaysPerMovie[0], rentalDaysPerMovie })
      .pipe(catchError(() => of(null)), finalize(() => { this.loading = false; this.cdr.detectChanges(); }))
      .subscribe({
        next: (rental: any) => {
          if (!rental?.id) { this.toastr.error('Could not create rental. Please try again.'); return; }
          this.showPaymentPopup = false;
          this.router.navigate(['/dashboard/pay'], {
            queryParams: {
              rentalId: rental.id,
              amount: this.total.toFixed(2),
              method: this.selectedPaymentMethod
            }
          });
        },
        error: () => this.toastr.error('Unable to create rental')
      });
  }

  goToMovies() { this.router.navigate(['/dashboard/movies']); }
  openMovie(movieId: number) { this.router.navigate(['/dashboard/movie', movieId]); }
}
