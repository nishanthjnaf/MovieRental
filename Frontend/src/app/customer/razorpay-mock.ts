import { ChangeDetectorRef, Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { PaymentService } from '../services/payment';
import { catchError, of } from 'rxjs';

@Component({
  selector: 'app-razorpay-mock',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './razorpay-mock.html'
})
export class RazorpayMock implements OnInit {
  rentalId = 0;
  amount = 0;
  method = 1;
  loading = false;

  // Card fields
  cardNumber = '';
  cardExpiry = '';
  cardCvv = '';
  cardName = '';

  // UPI field
  upiId = '';

  activeTab: 'card' | 'upi' | 'netbanking' = 'card';

  constructor(
    private route: ActivatedRoute,
    private router: Router,
    private paymentService: PaymentService,
    private cdr: ChangeDetectorRef
  ) {}

  ngOnInit() {
    this.route.queryParams.subscribe(params => {
      this.rentalId = Number(params['rentalId']) || 0;
      this.amount = Number(params['amount']) || 0;
      this.method = Number(params['method']) || 1;
      // Set default tab based on method
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
      error: () => {
        this.loading = false;
        this.cdr.detectChanges();
      }
    });
  }

  cancel() {
    this.router.navigate(['/dashboard/cart']);
  }
}
