import { ChangeDetectorRef, Component, OnInit } from '@angular/core';
import { CommonModule, DatePipe } from '@angular/common';
import { Router } from '@angular/router';
import { CurrentUserService } from '../services/current-user';
import { NotificationService } from '../services/notification';
import { ToastrService } from 'ngx-toastr';

@Component({
  selector: 'app-customer-notifications',
  standalone: true,
  imports: [CommonModule, DatePipe],
  templateUrl: './customer-notifications.html'
})
export class CustomerNotifications implements OnInit {
  notifications: any[] = [];
  loading = true;

  constructor(
    private currentUser: CurrentUserService,
    private notifService: NotificationService,
    private toastr: ToastrService,
    private router: Router,
    private cdr: ChangeDetectorRef
  ) {}

  ngOnInit() {
    this.load();
  }

  load() {
    const userId = this.currentUser.currentUserId || this.currentUser.decodedUserId;
    if (!userId) { this.loading = false; return; }

    this.notifService.getForUser(userId).subscribe({
      next: (res) => {
        this.notifications = res || [];
        this.loading = false;
        this.notifService.refreshUnread(userId);
        this.cdr.detectChanges();
      },
      error: () => { this.loading = false; this.cdr.detectChanges(); }
    });
  }

  markRead(n: any) {
    if (n.isRead) return;
    this.notifService.markRead(n.id).subscribe(() => {
      n.isRead = true;
      const userId = this.currentUser.currentUserId || this.currentUser.decodedUserId;
      this.notifService.refreshUnread(userId);
      this.cdr.detectChanges();
    });
  }

  markAllRead() {
    const userId = this.currentUser.currentUserId || this.currentUser.decodedUserId;
    if (!userId) return;
    this.notifService.markAllRead(userId).subscribe(() => {
      this.notifications.forEach(n => n.isRead = true);
      this.notifService.refreshUnread(userId);
      this.cdr.detectChanges();
    });
  }

  delete(n: any, event: Event) {
    event.stopPropagation();
    this.notifService.delete(n.id).subscribe(() => {
      this.notifications = this.notifications.filter(x => x.id !== n.id);
      const userId = this.currentUser.currentUserId || this.currentUser.decodedUserId;
      this.notifService.refreshUnread(userId);
      this.cdr.detectChanges();
    });
  }

  handleClick(n: any) {
    this.markRead(n);
    switch (n.type) {
      case 'new_movie':
      case 'rate_movie':
        if (n.relatedId) this.router.navigate(['/dashboard/movie', n.relatedId]);
        break;
      case 'expiry':
        // relatedId = movieId — go to movie detail so user can renew
        if (n.relatedId) this.router.navigate(['/dashboard/movie', n.relatedId]);
        break;
      case 'expired':
        // relatedId = rentalItemId — go to Expired section in My Rentals
        this.router.navigate(['/dashboard/rentals'], { queryParams: { section: 'expired' } });
        break;
      case 'payment':
        this.router.navigate(['/dashboard/transactions']);
        break;
      case 'refund':
        this.router.navigate(['/dashboard/rentals']);
        break;
      case 'password':
        this.router.navigate(['/dashboard/profile']);
        break;
      case 'admin_message':
      default:
        break;
    }
  }

  iconFor(type: string): string {
    switch (type) {
      case 'new_movie': return '🎬';
      case 'payment': return '💳';
      case 'expiry': return '⏰';
      case 'rate_movie': return '⭐';
      case 'refund': return '💰';
      case 'password': return '🔒';
      case 'admin_message': return '📢';
      default: return '🔔';
    }
  }

  get unreadCount(): number {
    return this.notifications.filter(n => !n.isRead).length;
  }
}
