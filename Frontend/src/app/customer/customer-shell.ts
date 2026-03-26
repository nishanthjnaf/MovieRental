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
        this.searchOrigin = this.currentPath.includes('/dashboard/home') ? 'home' : 'movies';
        this.router.navigate(['/dashboard/movies'], {
          queryParams: { q: term || null, page: 1 },
          queryParamsHandling: 'merge'
        });
      });

    this.router.events
      .pipe(filter((e) => e instanceof NavigationEnd))
      .subscribe(() => {
        this.currentPath = this.router.url;
      });
    this.currentPath = this.router.url;
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
    if (this.searchOrigin === 'home') {
      this.router.navigate(['/dashboard/home']);
      return;
    }
    this.router.navigate(['/dashboard/movies'], {
      queryParams: { q: null, page: 1 },
      queryParamsHandling: 'merge'
    });
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

