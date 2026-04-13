import { CanActivateFn, ActivatedRouteSnapshot, Router } from '@angular/router';
import { inject } from '@angular/core';
import { AuthService } from '../services/auth';
import { jwtDecode } from 'jwt-decode';

export const roleGuard: CanActivateFn = (route: ActivatedRouteSnapshot) => {
  const router = inject(Router);
  const auth = inject(AuthService);

  if (!auth.isTokenValid()) {
    auth.clearSession();
    router.navigate(['/login']);
    return false;
  }

  // Always derive role from the token — never trust a stale localStorage 'role' key
  let userRole: string | null = null;
  try {
    const token = auth.getToken();
    if (token) {
      const decoded: any = jwtDecode(token);
      userRole =
        decoded?.role ||
        decoded?.Role ||
        decoded?.['http://schemas.microsoft.com/ws/2008/06/identity/claims/role'] ||
        null;
      // Keep localStorage in sync so other parts of the app stay consistent
      if (userRole) localStorage.setItem('role', userRole);
    }
  } catch {
    userRole = localStorage.getItem('role');
  }

  const allowedRoles: string[] = route.data['roles'];

  if (allowedRoles.includes(userRole!)) {
    return true;
  }

  // Wrong role — redirect to the correct area
  if (userRole === 'Admin') {
    router.navigate(['/admin']);
  } else {
    router.navigate(['/dashboard']);
  }
  return false;
};