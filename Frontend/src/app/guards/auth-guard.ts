import { CanActivateFn, Router } from '@angular/router';
import { inject } from '@angular/core';
import { AuthService } from '../services/auth';

export const authGuard: CanActivateFn = () => {
  const router = inject(Router);
  const auth = inject(AuthService);

  if (auth.isTokenValid()) {
    return true; // ✅ token exists and is not expired
  }

  // token missing, expired, or tampered — clean up and redirect
  auth.clearSession();
  router.navigate(['/login']);
  return false;
};