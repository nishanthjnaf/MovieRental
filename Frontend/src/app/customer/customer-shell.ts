import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { NavigationEnd, Router, RouterModule } from '@angular/router';
import { FormControl, ReactiveFormsModule } from '@angular/forms';
import { debounceTime, distinctUntilChanged } from 'rxjs';
import { CurrentUserService } from '../services/current-user';
import { ToastrService } from 'ngx-toastr';
import { CartStateService } from '../services/cart-state';
import { filter } from 'rxjs/operators';
import { ThemeToggle } from '../components/theme-toggle';

@Component({
  selector: 'app-customer-shell',
  standalone: true,
  imports: [CommonModule, RouterModule, ReactiveFormsModule, ThemeToggle],
  templateUrl: './customer-shell.html',
  styleUrl: './customer-shell.css'
})
export class CustomerShell implements OnInit {
  searchControl = new FormControl('', { nonNullable: true });
  isSearchFocused = false;
  cartCount = 0;
  userName = 'Customer';
  isSidebarOpen = true;
  currentPath = '';
  searchOrigin: 'home' | 'movies' = 'movies';

  constructor(
    private router: Router,
    private toastr: ToastrService,
    private currentUser: CurrentUserService,
    private cart: CartStateService
  ) {}

  ngOnInit(): void {
    this.currentUser.loadCurrentUser().subscribe((u) => {
      this.userName = u?.username ?? 'Customer';
    });

    this.cart.reload();
    this.cart.cart$.subscribe((items) => (this.cartCount = items.length));

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

