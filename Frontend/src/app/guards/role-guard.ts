import { CanActivateFn, ActivatedRouteSnapshot, Router } from '@angular/router';
import { inject } from '@angular/core';
import { AuthService } from '../services/auth';

export const roleGuard: CanActivateFn = (route: ActivatedRouteSnapshot) => {
  const router = inject(Router);
  const auth = inject(AuthService);

  // Re-validate token here too — roleGuard can be used without authGuard in future
  if (!auth.isTokenValid()) {
    auth.clearSession();
    router.navigate(['/login']);
    return false;
  }

  const userRole = localStorage.getItem('role');
  const allowedRoles: string[] = route.data['roles'];

  if (allowedRoles.includes(userRole!)) {
    return true;
  }

  // Admin trying to access customer dashboard → send to admin panel
  if (userRole === 'Admin') {
    router.navigate(['/admin']);
  } else {
    router.navigate(['/dashboard']);
  }
  return false;
};