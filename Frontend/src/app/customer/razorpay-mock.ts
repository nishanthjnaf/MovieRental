import { ChangeDetectorRef, Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { PaymentService } from '../services/payment';
import { RentalService } from '../services/rental';
import { catchError, of } from 'rxjs';

@Component({
  selector: 'app-razorpay-mock',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './razorpay-mock.html'
})
export class RazorpayMock implements OnInit {
  rentalId = 0;
  rentalItemId = 0;
  days = 0;
  amount = 0;
  method = 1;
  source = ''; // 'renew' or ''
  loading = false;

  cardNumber = '';
  cardExpiry = '';
  cardCvv = '';
  cardName = '';
  upiId = '';

  activeTab: 'card' | 'upi' | 'netbanking' = 'card';

  constructor(
    private route: ActivatedRoute,
    private router: Router,
    private paymentService: PaymentService,
    private rentalService: RentalService,
    private cdr: ChangeDetectorRef
  ) {}

  ngOnInit() {
    this.route.queryParams.subscribe(params => {
      this.rentalId = Number(params['rentalId']) || 0;
      this.rentalItemId = Number(params['rentalItemId']) || 0;
      this.days = Number(params['days']) || 0;
      this.amount = Number(params['amount']) || 0;
      this.method = Number(params['method']) || 1;
      this.source = params['source'] || '';

      if (this.method === 3) this.activeTab = 'upi';
      else if (this.method === 2) this.activeTab = 'netbanking';
      else this.activeTab = 'card';
      this.cdr.detectChanges();
    });
  }

  pay(isSuccess: boolean) {
    if (this.loading) return;
    this.loading = true;
    this.cdr.detectChanges();

    if (this.source === 'renew') {
      this.handleRenew(isSuccess);
    } else {
      this.handlePayment(isSuccess);
    }
  }

  private handlePayment(isSuccess: boolean) {
    this.paymentService.makePayment({
      rentalId: this.rentalId,
      method: this.method,
      isSuccess
    }).pipe(catchError(() => of(null))).subscribe({
      next: () => {
        this.loading = false;
        this.router.navigate(['/dashboard/payment-result'], {
          queryParams: { success: isSuccess, amount: this.amount }
        });
      },
      error: () => { this.loading = false; this.cdr.detectChanges(); }
    });
  }

  private handleRenew(isSuccess: boolean) {
    if (!isSuccess) {
      this.loading = false;
      this.router.navigate(['/dashboard/payment-result'], {
        queryParams: { success: false, amount: this.amount, source: 'renew' }
      });
      return;
    }

    this.rentalService.renewItem(this.rentalItemId, this.days)
      .pipe(catchError(() => of(null)))
      .subscribe({
        next: () => {
          this.loading = false;
          this.router.navigate(['/dashboard/payment-result'], {
            queryParams: { success: true, amount: this.amount, source: 'renew' }
          });
        },
        error: () => { this.loading = false; this.cdr.detectChanges(); }
      });
  }

  cancel() {
    if (this.source === 'renew') {
      this.router.navigate(['/dashboard/rentals']);
    } else {
      this.router.navigate(['/dashboard/cart']);
    }
  }
}
