import { ChangeDetectorRef, Component, OnInit } from '@angular/core';
import { CommonModule, DatePipe } from '@angular/common';
import { CurrentUserService } from '../services/current-user';
import { PaymentService } from '../services/payment';
import { RentalService } from '../services/rental';
import { MovieService } from '../services/movie';
import { catchError, forkJoin, of } from 'rxjs';

@Component({
  selector: 'app-customer-transactions',
  standalone: true,
  imports: [CommonModule, DatePipe],
  templateUrl: './customer-transactions.html'
})
export class CustomerTransactions implements OnInit {
  transactions: any[] = [];
  loading = true;
  showMoviesPopup = false;
  selectedRentalId: number | null = null;
  selectedRentalMovies: Array<{ movieId: number; title: string }> = [];
  loadingRentalMovies = false;
  showRefundPopup = false;
  selectedRefund: any = null;

  constructor(
    private currentUser: CurrentUserService,
    private paymentService: PaymentService,
    private rentalService: RentalService,
    private movieService: MovieService,
    private cdr: ChangeDetectorRef
  ) {}

  ngOnInit(): void {
    this.loadTransactions();
  }

  loadTransactions() {
    const userId = this.currentUser.currentUserId || this.currentUser.decodedUserId;
    if (!userId) {
      this.transactions = [];
      this.loading = false;
      this.cdr.detectChanges();
      return;
    }
    this.loading = true;
    this.paymentService.getByUserId(userId).subscribe({
      next: (res: any) => {
        const list = Array.isArray(res) ? res : (res?.data ? [res.data] : []);
        this.transactions = list;
        this.loading = false;
        this.cdr.detectChanges();
      },
      error: () => {
        this.transactions = [];
        this.loading = false;
        this.cdr.detectChanges();
      }
    });
  }

  getPaymentMethod(method: number): string {
    switch (method) {
      case 0: return 'Debit Card';
      case 1: return 'Credit Card';
      case 2: return 'Net Banking';
      case 3: return 'UPI';
      default: return '-';
    }
  }

  getStatusText(status: number): string {
    switch (status) {
      case 0: return 'Success';
      case 1: return 'Failed';
      case 2: return 'Refunded';
      default: return '-';
    }
  }

  openRefundDetails(t: any) {
    // Fetch rental items to find which one was refunded, then get per-item refund
    this.rentalService.getItemsByRentalId(t.rentalId).subscribe({
      next: (items: any[]) => {
        // Try each item until we find one with a refund record
        const tryNext = (idx: number) => {
          if (idx >= items.length) {
            // Fallback: show payment-level refund data
            this.selectedRefund = t;
            this.showRefundPopup = true;
            this.cdr.detectChanges();
            return;
          }
          const itemId = items[idx]?.id ?? items[idx]?.rentalItemId;
          if (!itemId) { tryNext(idx + 1); return; }
          this.paymentService.getItemRefund(itemId).subscribe({
            next: (refund: any) => {
              this.selectedRefund = { ...t, refundAmount: refund.refundAmount, refundedAt: refund.refundedAt };
              this.showRefundPopup = true;
              this.cdr.detectChanges();
            },
            error: () => tryNext(idx + 1)
          });
        };
        tryNext(0);
      },
      error: () => {
        this.selectedRefund = t;
        this.showRefundPopup = true;
        this.cdr.detectChanges();
      }
    });
  }

  closeRefundDetails() {
    this.showRefundPopup = false;
    this.selectedRefund = null;
  }

  openRentalMovies(rentalId: number) {
    this.selectedRentalId = rentalId;
    this.selectedRentalMovies = [];
    this.showMoviesPopup = true;
    this.loadingRentalMovies = true;
    this.cdr.detectChanges();

    this.rentalService.getItemsByRentalId(rentalId).subscribe({
      next: (items: any[]) => {
        const movieIds = Array.from(new Set((items || []).map((i: any) => Number(i?.movieId)).filter((id) => id > 0)));
        if (!movieIds.length) {
          this.loadingRentalMovies = false;
          this.cdr.detectChanges();
          return;
        }

        forkJoin(
          movieIds.map((movieId) =>
            this.movieService.getById(movieId).pipe(
              catchError(() => of(null))
            )
          )
        ).subscribe((movies) => {
          this.selectedRentalMovies = movieIds.map((movieId, idx) => ({
            movieId,
            title: movies[idx]?.title || `Movie #${movieId}`
          }));
          this.loadingRentalMovies = false;
          this.cdr.detectChanges();
        });
      },
      error: () => {
        this.loadingRentalMovies = false;
        this.cdr.detectChanges();
      }
    });
  }

  closeRentalMovies() {
    this.showMoviesPopup = false;
    this.selectedRentalId = null;
    this.selectedRentalMovies = [];
  }
}
