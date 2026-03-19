import { CanActivateFn, ActivatedRouteSnapshot, Router } from '@angular/router';
import { inject } from '@angular/core';

export const roleGuard: CanActivateFn = (route: ActivatedRouteSnapshot) => {

  const router = inject(Router);

  const userRole = localStorage.getItem('role');
  const allowedRoles = route.data['roles'];

  if (allowedRoles.includes(userRole)) {
    return true;
  } else {
    router.navigate(['/dashboard']);
    return false;
  }
};