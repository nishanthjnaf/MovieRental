import { ChangeDetectorRef, Component, OnInit } from '@angular/core';
import { CommonModule, DatePipe } from '@angular/common';
import { UserService } from '../services/user';
import { CurrentUserService } from '../services/current-user';
import { RentalService } from '../services/rental';
import { FormsModule } from '@angular/forms';
import { ToastrService } from 'ngx-toastr';
import { PaymentService } from '../services/payment';
import { Router } from '@angular/router';

@Component({
  selector: 'app-customer-rentals',
  standalone: true,
  imports: [CommonModule, DatePipe, FormsModule],
  templateUrl: './customer-rentals.html'
})
export class CustomerRentals implements OnInit {
  rentals: any[] = [];
  activeRentals: any[] = [];
  returnedRentals: any[] = [];
  expiredRentals: any[] = [];
  loading = true;
  showDetailsPopup = false;
  selectedRentalDetails: any = null;
  showRenewPopup = false;
  renewDays = 1;
  selectedRentalForRenew: any = null;

  constructor(
    private userService: UserService,
    private currentUser: CurrentUserService,
    private rentalService: RentalService,
    private paymentService: PaymentService,
    private toastr: ToastrService,
    private router: Router,
    private cdr: ChangeDetectorRef
  ) {}

  ngOnInit(): void {
    this.load();
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
        this.rentals = (items || []).map((r: any) => ({
          ...r,
          rentalItemId: r.rentalItemId ?? r.id
        }));
        this.splitRentals();
        this.loading = false;
        this.cdr.detectChanges();
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
    this.showRenewPopup = true;
  }

  closeRenew() {
    this.showRenewPopup = false;
    this.selectedRentalForRenew = null;
    this.renewDays = 1;
  }

  renew() {
    if (!this.selectedRentalForRenew) return;
    const daysToAdd = Number(this.renewDays || 0);
    if (!daysToAdd || daysToAdd <= 0) {
      this.toastr.error('Enter valid days');
      return;
    }
    this.rentalService.renewItem(this.selectedRentalForRenew.rentalItemId, daysToAdd).subscribe({
      next: () => {
        this.toastr.success('Rental renewed');
        const current = new Date(this.selectedRentalForRenew.endDate);
        current.setDate(current.getDate() + daysToAdd);
        this.selectedRentalForRenew.endDate = current.toISOString();
        this.selectedRentalForRenew.isActive = true;
        this.splitRentals();
        this.closeRenew();
      },
      error: () => this.toastr.error('Renew failed')
    });
  }

  endRental(item: any) {
    this.rentalService.endItem(item.rentalItemId).subscribe({
      next: () => {
        this.toastr.success(`"${item.movieTitle}" rental ended`);
        item.isActive = false;
        this.splitRentals();
        this.cdr.detectChanges();
        this.load();
      },
      error: (err) => {
        const msg = String(err?.error || '').toLowerCase();
        const alreadyEnded = err?.status === 409 || msg.includes('already ended');
        if (alreadyEnded) {
          this.toastr.success(`"${item.movieTitle}" rental ended`);
          item.isActive = false;
          this.splitRentals();
          this.cdr.detectChanges();
          this.load();
          return;
        }
        this.toastr.error('Could not end rental');
      }
    });
  }

  openDetails(item: any) {
    this.paymentService.getByRentalId(item.rentalId).subscribe({
      next: (payment: any) => {
        this.selectedRentalDetails = {
          movieTitle: item.movieTitle,
          amountPaid: payment?.amount ?? (item.pricePerDay || 0),
          purchaseDate: payment?.paymentDate || item.startDate,
          expiryDate: item.endDate,
          paymentMethod: this.getPaymentMethod(payment?.paymentMethod ?? payment?.method)
        };
        this.showDetailsPopup = true;
        this.cdr.detectChanges();
      },
      error: () => {
        this.selectedRentalDetails = {
          movieTitle: item.movieTitle,
          amountPaid: item.pricePerDay || 0,
          purchaseDate: item.startDate,
          expiryDate: item.endDate,
          paymentMethod: '-'
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

  watchMovie(movieId: number) {
    this.router.navigate(['/dashboard/watch', movieId]);
  }
}

