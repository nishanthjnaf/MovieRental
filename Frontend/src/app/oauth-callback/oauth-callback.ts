import { Component, OnInit } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { jwtDecode } from 'jwt-decode';
import { CommonModule } from '@angular/common';
import { UserService } from '../services/user';
import { CartStateService } from '../services/cart-state';
import { catchError, of } from 'rxjs';

/**
 * Landing page after Google/Facebook OAuth redirect.
 * The backend redirects here with ?token=<jwt>
 * We store it and navigate just like a normal login.
 */
@Component({
  selector: 'app-oauth-callback',
  standalone: true,
  imports: [CommonModule],
  template: `<div style="display:flex;align-items:center;justify-content:center;height:100vh;color:#fff;font-size:1.1rem;">
    Signing you in...
  </div>`
})
export class OAuthCallback implements OnInit {
  constructor(
    private route: ActivatedRoute,
    private router: Router,
    private userService: UserService,
    private cartState: CartStateService
  ) {}

  ngOnInit() {
    const token = this.route.snapshot.queryParamMap.get('token');
    const error = this.route.snapshot.queryParamMap.get('error');

    if (error || !token) {
      this.router.navigate(['/login'], { queryParams: { error: error ?? 'OAuth failed' } });
      return;
    }

    // Store token (OAuth logins are always "remembered" — use localStorage)
    localStorage.setItem('token', token);

    const decoded: any = jwtDecode(token);
    const role = decoded.role
      || decoded.Role
      || decoded['http://schemas.microsoft.com/ws/2008/06/identity/claims/role'];
    localStorage.setItem('role', role);

    this.cartState.reload();

    if (role === 'Admin') {
      this.router.navigate(['/admin']);
      return;
    }

    const nameId = decoded?.nameid
      || decoded?.sub
      || decoded?.['http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier'];
    const userId = Number(nameId);

    if (userId > 0) {
      this.userService.getPreferences(userId).pipe(catchError(() => of(null))).subscribe(pref => {
        if (!pref || !pref.isSet) {
          this.router.navigate(['/preferences']);
        } else {
          this.router.navigate(['/dashboard']);
        }
      });
    } else {
      this.router.navigate(['/dashboard']);
    }
  }
}
