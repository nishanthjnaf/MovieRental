import { CanActivateFn, Router } from '@angular/router';
import { inject } from '@angular/core';

export const authGuard: CanActivateFn = () => {

  const router = inject(Router);
  const token =
  localStorage.getItem('token') ||
  sessionStorage.getItem('token');

  if (token) {
    return true; // ✅ allow access
  } else {
    router.navigate(['/login']); // ❌ redirect
    return false;
  }
};