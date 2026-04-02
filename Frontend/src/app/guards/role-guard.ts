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

  router.navigate(['/dashboard']);
  return false;
};