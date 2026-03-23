import { Routes } from '@angular/router';
import { Login } from './login/login';
import { Register } from './register/register';
import { authGuard } from './guards/auth-guard';
import { roleGuard } from './guards/role-guard';
import { Admin } from './admin/admin';
import { CustomerShell } from './customer/customer-shell';
import { CustomerHome } from './customer/customer-home';
import { CustomerMovieDetail } from './customer/customer-movie-detail';
import { CustomerRentals } from './customer/customer-rentals';
import { CustomerWatchlist } from './customer/customer-watchlist';
import { CustomerCart } from './customer/customer-cart';
import { CustomerProfile } from './customer/customer-profile';
import { CustomerMovies } from './customer/customer-movies';
import { CustomerMyRatings } from './customer/customer-my-ratings';
import { CustomerTransactions } from './customer/customer-transactions';
import { CustomerWatch } from './customer/customer-watch';
import { AdminProfile } from './admin/admin-profile';




export const routes: Routes = [

  { path: '', redirectTo: 'login', pathMatch: 'full' },

  { path: 'login', component: Login },
  { path: 'register', component: Register },

  {
    path: 'dashboard',
    component: CustomerShell,
    canActivate: [authGuard],
    children: [
      { path: '', redirectTo: 'home', pathMatch: 'full' },
      { path: 'home', component: CustomerHome },
      { path: 'movies', component: CustomerMovies },
      { path: 'search', redirectTo: 'movies', pathMatch: 'full' },
      { path: 'movie/:id', component: CustomerMovieDetail },
      { path: 'rentals', component: CustomerRentals },
      { path: 'watchlist', component: CustomerWatchlist },
      { path: 'my-ratings', component: CustomerMyRatings },
      { path: 'transactions', component: CustomerTransactions },
      { path: 'cart', component: CustomerCart },
      { path: 'profile', component: CustomerProfile },
      { path: 'watch/:id', component: CustomerWatch }
    ]
  },

  {
    path: 'admin',
    component: Admin, // ✅ ADMIN PAGE
    canActivate: [authGuard, roleGuard],
    data: { roles: ['Admin'] }
  },
  {
    path: 'admin/profile',
    component: AdminProfile,
    canActivate: [authGuard, roleGuard],
    data: { roles: ['Admin'] }
  }

];