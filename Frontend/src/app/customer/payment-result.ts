import { ChangeDetectorRef, Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ActivatedRoute, Router } from '@angular/router';
import { CartStateService } from '../services/cart-state';
import { CurrentUserService } from '../services/current-user';
import { NotificationService } from '../services/notification';

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
    private currentUser: CurrentUserService,
    private notifService: NotificationService,
    private cdr: ChangeDetectorRef
  ) {}

  ngOnInit() {
    this.route.queryParams.subscribe(params => {
      this.success = params['success'] === 'true';
      this.amount = Number(params['amount']) || 0;
      const source = params['source'] || '';
      if (this.success && source !== 'renew') {
        this.cart.clear();
      }
      // Refresh notifications immediately after payment result
      const uid = this.currentUser.currentUserId || this.currentUser.decodedUserId;
      if (uid) this.notifService.refreshUnread(uid);
      this.cdr.detectChanges();
    });
  }

  close() {
    this.router.navigate(['/dashboard/rentals']);
  }
}
