import { ChangeDetectorRef, Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ActivatedRoute, Router } from '@angular/router';
import { CartStateService } from '../services/cart-state';

@Component({
  selector: 'app-payment-result',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './payment-result.html'
})
export class PaymentResult implements OnInit {
  success = false;
  amount = 0;

  constructor(
    private route: ActivatedRoute,
    private router: Router,
    private cart: CartStateService,
    private cdr: ChangeDetectorRef
  ) {}

  ngOnInit() {
    this.route.queryParams.subscribe(params => {
      this.success = params['success'] === 'true';
      this.amount = Number(params['amount']) || 0;
      if (this.success) {
        this.cart.clear();
      }
      this.cdr.detectChanges();
    });
  }

  close() {
    this.router.navigate(['/dashboard/rentals']);
  }
}
