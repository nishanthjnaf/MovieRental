import { ChangeDetectorRef, Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { NavigationEnd, Router, RouterModule } from '@angular/router';
import { FormControl, ReactiveFormsModule } from '@angular/forms';
import { debounceTime, distinctUntilChanged } from 'rxjs';
import { CurrentUserService } from '../services/current-user';
import { ToastrService } from 'ngx-toastr';
import { CartStateService } from '../services/cart-state';
import { NotificationService } from '../services/notification';
import { filter } from 'rxjs/operators';

@Component({
  selector: 'app-customer-shell',
  standalone: true,
  imports: [CommonModule, RouterModule, ReactiveFormsModule],
  templateUrl: './customer-shell.html',
  styleUrl: './customer-shell.css'
})
export class CustomerShell implements OnInit {
  searchControl = new FormControl('', { nonNullable: true });
  isSearchFocused = false;
  cartCount = 0;
  unreadNotifications = 0;
  recentNotifications: any[] = [];
  showNotifDropdown = false;
  userName = 'Customer';
  isSidebarOpen = true;
  currentPath = '';
  searchOrigin: 'home' | 'movies' = 'movies';
  private currentUserId = 0;

  constructor(
    private router: Router,
    private toastr: ToastrService,
    private currentUser: CurrentUserService,
    private cart: CartStateService,
    private notifService: NotificationService,
    private cdr: ChangeDetectorRef
  ) {}

  ngOnInit(): void {
    this.currentUser.loadCurrentUser().subscribe((u) => {
      this.userName = u?.username ?? 'Customer';
      if (u?.id) {
        this.currentUserId = u.id;
        this.notifService.refreshUnread(u.id);
        // Poll every 10s for near-instant notification updates
        setInterval(() => this.notifService.refreshUnread(u.id), 10000);
      }
    });

    this.notifService.unread$.subscribe(count => {
      this.unreadNotifications = count;
      this.cdr.detectChanges();
    });

    this.cart.reload();
    this.cart.cart$.subscribe((items) => {
      this.cartCount = items.length;
      this.cdr.detectChanges();
    });

    this.searchControl.valueChanges
      .pipe(debounceTime(200), distinctUntilChanged())
      .subscribe((term) => {
        if (!term?.trim()) return;
        this.router.navigate(['/dashboard/search'], {
          queryParams: { q: term.trim() },
        });
      });

    this.router.events
      .pipe(filter((e) => e instanceof NavigationEnd))
      .subscribe(() => {
        this.currentPath = this.router.url;
      });
    this.currentPath = this.router.url;
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

  toggleNotifDropdown() {
    // If already open, just close
    if (this.showNotifDropdown) {
      this.showNotifDropdown = false;
      return;
    }

    // Resolve uid — try all sources
    const uid = this.currentUserId || this.currentUser.currentUserId || this.currentUser.decodedUserId;

    const fetchAndShow = (resolvedUid: number) => {
      this.currentUserId = resolvedUid;
      this.notifService.getForUser(resolvedUid).subscribe(res => {
        this.recentNotifications = (res || []).slice(0, 5);
        this.notifService.refreshUnread(resolvedUid);
        this.showNotifDropdown = true;
        this.cdr.detectChanges();
        this.cdr.markForCheck();
      });
    };

    if (uid > 0) {
      fetchAndShow(uid);
    } else {
      // Wait for user to resolve, then fetch
      this.currentUser.loadCurrentUser().subscribe(u => {
        const resolvedId = u?.id || this.currentUser.currentUserId || this.currentUser.decodedUserId;
        if (resolvedId > 0) fetchAndShow(resolvedId);
      });
    }
  }

  private loadDropdownNotifs(uid: number) {
    this.notifService.getForUser(uid).subscribe(res => {
      this.recentNotifications = (res || []).slice(0, 5);
      this.notifService.refreshUnread(uid);
    });
  }

  handleNotifClick(n: any) {
    // Mark as read
    if (!n.isRead) {
      this.notifService.markRead(n.id).subscribe(() => {
        n.isRead = true;
        this.notifService.refreshUnread(this.currentUserId);
      });
    }
    this.closeNotifDropdown();
    // Route based on type
    switch (n.type) {
      case 'new_movie': case 'rate_movie':
        if (n.relatedId) this.router.navigate(['/dashboard/movie', n.relatedId]); break;
      case 'payment': this.router.navigate(['/dashboard/transactions']); break;
      case 'expiry': case 'refund': this.router.navigate(['/dashboard/rentals']); break;
      case 'password': this.router.navigate(['/dashboard/profile']); break;
    }
  }

  closeNotifDropdown() {
    this.showNotifDropdown = false;
  }

  openNotifPage() {
    this.showNotifDropdown = false;
    this.router.navigate(['/dashboard/notifications']);
  }

  go(path: string) {
    this.router.navigate([path]);
  }

  toggleSidebar() {
    this.isSidebarOpen = !this.isSidebarOpen;
  }

  clearSearch() {
    this.searchControl.setValue('', { emitEvent: false });
    this.router.navigate(['/dashboard/home']);
  }

  logout() {
    localStorage.removeItem('token');
    sessionStorage.removeItem('token');
    localStorage.removeItem('role');
    this.currentUser.clear();
    this.cart.reset();
    this.toastr.info('Logged out');
    this.router.navigate(['/login']);
  }
}

