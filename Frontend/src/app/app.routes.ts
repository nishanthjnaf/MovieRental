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
import { RazorpayMock } from './customer/razorpay-mock';
import { PaymentResult } from './customer/payment-result';
import { CustomerPreferencesPage } from './customer/customer-preferences-page';
import { AdminProfile } from './admin/admin-profile';
import { OAuthCallback } from './oauth-callback/oauth-callback';
import { CustomerNotifications } from './customer/customer-notifications';
import { CustomerSeries } from './customer/customer-series';
import { CustomerSeriesDetail } from './customer/customer-series-detail';
import { CustomerWatchSeries } from './customer/customer-watch-series';
import { CustomerSearch } from './customer/customer-search';
import { NotFound } from './not-found';

export const routes: Routes = [

  { path: '', redirectTo: 'login', pathMatch: 'full' },
  { path: 'login', component: Login },
  { path: 'register', component: Register },
  { path: 'oauth-callback', component: OAuthCallback },
  { path: 'preferences', component: CustomerPreferencesPage, canActivate: [authGuard] },

  {
    path: 'dashboard',
    component: CustomerShell,
    canActivate: [authGuard, roleGuard],
    data: { roles: ['Customer'] },
    children: [
      { path: '', redirectTo: 'home', pathMatch: 'full' },
      { path: 'home', component: CustomerHome },
      { path: 'movies', component: CustomerMovies },
      { path: 'search', component: CustomerSearch },
      { path: 'movie/:id', component: CustomerMovieDetail },
      { path: 'series', component: CustomerSeries },
      { path: 'series/:id', component: CustomerSeriesDetail },
      { path: 'watch-series/:id', component: CustomerWatchSeries },
      { path: 'rentals', component: CustomerRentals },
      { path: 'watchlist', component: CustomerWatchlist },
      { path: 'my-ratings', component: CustomerMyRatings },
      { path: 'transactions', component: CustomerTransactions },
      { path: 'cart', component: CustomerCart },
      { path: 'profile', component: CustomerProfile },
      { path: 'watch/:id', component: CustomerWatch },
      { path: 'pay', component: RazorpayMock },
      { path: 'payment-result', component: PaymentResult },
      { path: 'notifications', component: CustomerNotifications }
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
  },

  { path: '**', component: NotFound }

];