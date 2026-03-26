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
    if (n.type === 'new_movie' && n.relatedId) this.router.navigate(['/dashboard/movie', n.relatedId]);
    if (n.type === 'rate_movie' && n.relatedId) this.router.navigate(['/dashboard/movie', n.relatedId]);
    if (n.type === 'expiry') this.router.navigate(['/dashboard/rentals']);
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
