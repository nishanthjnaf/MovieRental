import { Component, OnInit } from '@angular/core';
import { FormBuilder, Validators, ReactiveFormsModule } from '@angular/forms';
import { AuthService } from '../services/auth';
import { CommonModule } from '@angular/common';
import { Router, RouterModule } from '@angular/router';
import { ToastrService } from 'ngx-toastr';
import { jwtDecode } from 'jwt-decode';
import { CartStateService } from '../services/cart-state';
import { ThemeToggle } from '../components/theme-toggle';
import { UserService } from '../services/user';
import { catchError, of } from 'rxjs';

@Component({
  selector: 'app-login',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, RouterModule, ThemeToggle],
  templateUrl: './login.html'
})
export class Login implements OnInit {
  form: any;
  showPassword = false;

  constructor(
    private fb: FormBuilder,
    private auth: AuthService,
    private router: Router,
    private toastr: ToastrService,
    private cartState: CartStateService,
    private userService: UserService
  ) {}

  togglePassword() {
    this.showPassword = !this.showPassword;
  }

  ngOnInit() {
    const token = localStorage.getItem('token') || sessionStorage.getItem('token');
    const role = localStorage.getItem('role');
    if (token && role) {
      if (role === 'Admin') this.router.navigate(['/admin']);
      else this.router.navigate(['/dashboard']);
    }
    this.form = this.fb.group({
      username: ['', Validators.required],
      password: ['', Validators.required],
      remember: [false]
    });
  }

  get f() { return this.form.controls; }

  submit() {
    if (this.form.invalid) return;

    this.auth.login(this.form.value).subscribe({
      next: (res: any) => {
        this.toastr.success('Login successful');
        const token = res.token;
        if (this.form.value.remember) {
          localStorage.setItem('token', token);
        } else {
          sessionStorage.setItem('token', token);
        }
        const decoded: any = jwtDecode(token);
        const role = decoded.role || decoded.Role || decoded['http://schemas.microsoft.com/ws/2008/06/identity/claims/role'];
        localStorage.setItem('role', role);
        this.cartState.reload();

        if (role === 'Admin') {
          this.router.navigate(['/admin']);
          return;
        }

        // For customers: check if preferences are set
        const nameId =
          decoded?.nameid ||
          decoded?.sub ||
          decoded?.['http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier'];
        const userId = Number(nameId);

        if (userId > 0) {
          this.userService.getPreferences(userId).pipe(catchError(() => of(null))).subscribe(pref => {
            if (!pref || !pref.isSet) {
              this.router.navigate(['/preferences']);
            } else {
              // Apply saved theme
              if (pref.theme === 'light') {
                document.documentElement.classList.add('light-theme');
                document.documentElement.classList.remove('dark-theme');
                localStorage.setItem('cinefilia_theme', 'light');
              }
              this.router.navigate(['/dashboard']);
            }
          });
        } else {
          this.router.navigate(['/dashboard']);
        }
      },
      error: () => {
        this.toastr.error('Invalid credentials');
      }
    });
  }
}