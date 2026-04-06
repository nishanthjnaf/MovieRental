import { ChangeDetectorRef, Component, OnInit, ElementRef, ViewChild, AfterViewInit } from '@angular/core';
import { CommonModule, DatePipe } from '@angular/common';
import { UserService } from '../services/user';
import { CurrentUserService } from '../services/current-user';
import { RentalService } from '../services/rental';
import { SeriesService } from '../services/series';
import { FormsModule } from '@angular/forms';
import { ToastrService } from 'ngx-toastr';
import { PaymentService } from '../services/payment';
import { NotificationService } from '../services/notification';
import { Router, ActivatedRoute } from '@angular/router';
@Component({
  selector: 'app-customer-rentals',
  standalone: true,
  imports: [CommonModule, DatePipe, FormsModule],
  templateUrl: './customer-rentals.html'
})
export class CustomerRentals implements OnInit, AfterViewInit {
  @ViewChild('expiredSection') expiredSectionRef!: ElementRef;

  rentals: any[] = [];
  activeRentals: any[] = [];
  returnedRentals: any[] = [];
  expiredRentals: any[] = [];
  loading = true;
  showDetailsPopup = false;
  selectedRentalDetails: any = null;
  showRenewPopup = false;
  renewDays = 1;
  renewStep = 1;
  renewPaymentMethod = 1;
  selectedRentalForRenew: any = null;

  // Refund confirmation
  showRefundPopup = false;
  selectedRentalForRefund: any = null;
  refundPercent = 0;
  refundAmount = 0;
  refundTotal = 0;

  // Search
  searchQuery = '';

  // Filter: 'all' | 'movies' | 'series'
  rentalFilter: 'all' | 'movies' | 'series' = 'all';

  get filteredActive(): any[] { return this.applyTypeFilter(this.activeRentals); }
  get filteredReturned(): any[] { return this.applyTypeFilter(this.returnedRentals); }
  get filteredExpired(): any[] { return this.applyTypeFilter(this.expiredRentals); }

  private applyTypeFilter(list: any[]): any[] {
    if (this.rentalFilter === 'movies') return list.filter(r => !r._isSeries);
    if (this.rentalFilter === 'series') return list.filter(r => r._isSeries);
    return list;
  }

  get searchResults(): any[] {
    const q = this.searchQuery.trim().toLowerCase();
    if (!q) return [];
    return this.applyTypeFilter(this.rentals).filter(r =>
      (r.movieTitle || r.seriesTitle || '').toLowerCase().includes(q)
    );
  }

  get isSearching(): boolean {
    return this.searchQuery.trim().length > 0;
  }

  constructor(
    private userService: UserService,
    private currentUser: CurrentUserService,
    private rentalService: RentalService,
    private seriesService: SeriesService,
    private paymentService: PaymentService,
    private notifService: NotificationService,
    private toastr: ToastrService,
    private router: Router,
    private route: ActivatedRoute,
    private cdr: ChangeDetectorRef
  ) {}

  ngOnInit(): void {
    this.load();
  }

  ngAfterViewInit(): void {
    this.route.queryParamMap.subscribe(params => {
      if (params.get('section') === 'expired') {
        // Wait for data to load then scroll
        const tryScroll = () => {
          if (this.expiredSectionRef?.nativeElement) {
            this.expiredSectionRef.nativeElement.scrollIntoView({ behavior: 'smooth', block: 'start' });
          } else {
            setTimeout(tryScroll, 300);
          }
        };
        setTimeout(tryScroll, 400);
      }
    });
  }

  load() {
    const userId = this.currentUser.currentUserId || this.currentUser.decodedUserId;
    if (!userId) {
      this.loading = false;
      this.cdr.detectChanges();
      return;
    }
    this.userService.getRentedMovies(userId).subscribe({
      next: (items) => {
        const movieRentals = (items || []).map((r: any) => ({
          ...r,
          rentalItemId: r.rentalItemId ?? r.id,
          _isSeries: false,
          displayTitle: r.movieTitle
        }));

        this.seriesService.getRentalsByUser(userId).subscribe({
          next: (seriesItems) => {
            const seriesRentals = (seriesItems || []).map((r: any) => ({
              ...r,
              rentalItemId: r.id,
              movieTitle: r.seriesTitle,
              movieId: r.seriesId,
              _isSeries: true,
              displayTitle: r.seriesTitle
            }));
            this.rentals = [...movieRentals, ...seriesRentals];
            this.splitRentals();
            this.loading = false;
            this.cdr.detectChanges();
          },
          error: () => {
            this.rentals = movieRentals;
            this.splitRentals();
            this.loading = false;
            this.cdr.detectChanges();
          }
        });
      },
      error: () => {
        this.rentals = [];
        this.activeRentals = [];
        this.returnedRentals = [];
        this.expiredRentals = [];
        this.loading = false;
        this.cdr.detectChanges();
      }
    });
  }

  private splitRentals() {
    const now = new Date();
    this.activeRentals = [];
    this.returnedRentals = [];
    this.expiredRentals = [];

    for (const rental of this.rentals) {
      const endDate = new Date(rental.endDate);
      if (endDate <= now) {
        this.expiredRentals.push({ ...rental, isActive: false });
      } else if (rental.isActive) {
        this.activeRentals.push(rental);
      } else {
        this.returnedRentals.push(rental);
      }
    }
  }

  openRenew(item: any) {
    this.selectedRentalForRenew = item;
    this.renewDays = 1;
    this.renewStep = 1;
    this.renewPaymentMethod = 1;
    this.showRenewPopup = true;
  }

  closeRenew() {
    this.showRenewPopup = false;
    this.selectedRentalForRenew = null;
    this.renewDays = 1;
    this.renewStep = 1;
  }

  get renewTotal(): number {
    const price = Number(this.selectedRentalForRenew?.pricePerDay || 0);
    const days = Math.max(1, Number(this.renewDays || 1));
    return price * days;
  }

  renewContinue() {
    const days = Number(this.renewDays || 0);
    if (!days || days <= 0) { this.toastr.error('Enter valid days'); return; }
    this.renewStep = 2;
    this.cdr.detectChanges();
  }

  renewProceed() {
    if (!this.selectedRentalForRenew) return;
    const days = Number(this.renewDays || 0);
    if (!days || days <= 0) return;
    this.router.navigate(['/dashboard/pay'], {
      queryParams: {
        source: 'renew',
        rentalItemId: this.selectedRentalForRenew.rentalItemId,
        days,
        amount: this.renewTotal.toFixed(2),
        method: this.renewPaymentMethod
      }
    });
    this.closeRenew();
  }

  renew() {
    // kept for backward compat but no longer used directly
  }

  openEndRental(item: any) {
    const days = Math.max(1, Math.round((new Date(item.endDate).getTime() - new Date(item.startDate).getTime()) / 86400000));
    const itemTotal = item.totalAmount ?? (item.pricePerDay * days);

    this.paymentService.getByRentalId(item.rentalId).subscribe({
      next: (payment: any) => {
        const purchaseDate = new Date(payment?.paymentDate || item.startDate);
        const now = new Date();
        const hoursSince = (now.getTime() - purchaseDate.getTime()) / (1000 * 60 * 60);
        this.refundPercent = hoursSince <= 2 ? 75 : hoursSince <= 4 ? 50 : 0;
        this.refundTotal = itemTotal;
        this.refundAmount = this.refundTotal * (this.refundPercent / 100);
        this.selectedRentalForRefund = item;
        this.showRefundPopup = true;
        this.cdr.detectChanges();
      },
      error: () => {
        this.refundPercent = 0;
        this.refundTotal = itemTotal;
        this.refundAmount = 0;
        this.selectedRentalForRefund = item;
        this.showRefundPopup = true;
        this.cdr.detectChanges();
      }
    });
  }

  closeRefundPopup() {
    this.showRefundPopup = false;
    this.selectedRentalForRefund = null;
    this.cdr.detectChanges();
  }

  confirmEndRental() {
    const item = this.selectedRentalForRefund;
    if (!item) return;
    this.closeRefundPopup();

    this.paymentService.processRefund(item.rentalItemId).subscribe({
      next: () => {
        this.toastr.success(
          this.refundPercent > 0
            ? `Rental ended. ₹${this.refundAmount.toFixed(2)} refunded.`
            : 'Rental ended. No refund applicable.'
        );
        const uid = this.currentUser.currentUserId || this.currentUser.decodedUserId;
        if (uid) this.notifService.refreshUnread(uid);
        this.load();
      },
      error: () => {
        this.rentalService.endItem(item.rentalItemId).subscribe({
          next: () => { this.toastr.success('Rental ended'); this.load(); },
          error: () => this.toastr.error('Could not end rental')
        });
      }
    });
  }

  endRental(item: any) {
    this.openEndRental(item);
  }

  openDetails(item: any) {
    const days = Math.max(1, Math.round((new Date(item.endDate).getTime() - new Date(item.startDate).getTime()) / 86400000));
    const itemTotal = item.totalAmount ?? (item.pricePerDay * days);

    // Fetch payment (for method/date) and per-item refund in parallel
    this.paymentService.getByRentalId(item.rentalId).subscribe({
      next: (payment: any) => {
        // Now fetch per-item refund separately
        this.paymentService.getItemRefund(item.rentalItemId).subscribe({
          next: (refund: any) => {
            this.selectedRentalDetails = {
              movieTitle: item.movieTitle,
              amountPaid: itemTotal,
              purchaseDate: payment?.paymentDate || item.startDate,
              expiryDate: item.endDate,
              paymentMethod: this.getPaymentMethod(payment?.paymentMethod ?? payment?.method),
              refundAmount: refund?.refundAmount ?? null,
              refundedAt: refund?.refundedAt ?? null
            };
            this.showDetailsPopup = true;
            this.cdr.detectChanges();
          },
          error: () => {
            // No refund for this item
            this.selectedRentalDetails = {
              movieTitle: item.movieTitle,
              amountPaid: itemTotal,
              purchaseDate: payment?.paymentDate || item.startDate,
              expiryDate: item.endDate,
              paymentMethod: this.getPaymentMethod(payment?.paymentMethod ?? payment?.method),
              refundAmount: null,
              refundedAt: null
            };
            this.showDetailsPopup = true;
            this.cdr.detectChanges();
          }
        });
      },
      error: () => {
        this.selectedRentalDetails = {
          movieTitle: item.movieTitle,
          amountPaid: itemTotal,
          purchaseDate: item.startDate,
          expiryDate: item.endDate,
          paymentMethod: '-',
          refundAmount: null,
          refundedAt: null
        };
        this.showDetailsPopup = true;
        this.cdr.detectChanges();
      }
    });
  }

  closeDetails() {
    this.showDetailsPopup = false;
    this.selectedRentalDetails = null;
  }

  private getPaymentMethod(method: number | undefined): string {
    switch (method) {
      case 0: return 'Debit Card';
      case 1: return 'Credit Card';
      case 2: return 'Net Banking';
      case 3: return 'UPI';
      default: return '-';
    }
  }

  openMovie(movieId: number) {
    this.router.navigate(['/dashboard/movie', movieId]);
  }

  openItem(item: any) {
    if (item._isSeries) {
      this.router.navigate(['/dashboard/series', item.movieId || item.seriesId]);
    } else {
      this.router.navigate(['/dashboard/movie', item.movieId]);
    }
  }

  getStatusLabel(r: any): string {
    const now = new Date();
    if (new Date(r.endDate) <= now) return 'expired';
    if (r.isActive) return 'active';
    return 'returned';
  }

  getExpiryCountdown(endDate: string): string {
    const diff = new Date(endDate).getTime() - Date.now();
    if (diff <= 0) return 'Expired';
    const hours = diff / (1000 * 60 * 60);
    if (hours < 24) return `${Math.ceil(hours)}h left`;
    const days = Math.ceil(hours / 24);
    return `${days} day${days === 1 ? '' : 's'} left`;
  }

  isExpiringSoon(endDate: string): boolean {
    const diff = new Date(endDate).getTime() - Date.now();
    return diff > 0 && diff < 24 * 60 * 60 * 1000;
  }

  watchMovie(movieId: number) {
    this.router.navigate(['/dashboard/watch', movieId]);
  }

  watchItem(r: any) {
    if (r._isSeries) this.router.navigate(['/dashboard/watch-series', r.movieId || r.seriesId]);
    else this.router.navigate(['/dashboard/watch', r.movieId]);
  }
}

