import { Component, OnInit } from '@angular/core';
import { AbstractControl, FormBuilder, ReactiveFormsModule, ValidationErrors, ValidatorFn, Validators } from '@angular/forms';
import { AuthService } from '../services/auth';
import { Router, RouterModule } from '@angular/router';
import { CommonModule } from '@angular/common';
import { ToastrService } from 'ngx-toastr';
import { ThemeToggle } from '../components/theme-toggle';

@Component({
  selector: 'app-register',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, RouterModule, ThemeToggle],
  templateUrl: './register.html'
})
export class Register implements OnInit {

  form: any;

  constructor(
    private fb: FormBuilder,
    private auth: AuthService,
    private router: Router,
    private toastr: ToastrService
  ) {}

  ngOnInit() {
    this.form = this.fb.group({
      username: ['', [Validators.required, Validators.pattern('^[a-z]{4,}$')]],
      password: ['', [
        Validators.required,
        Validators.pattern('^(?=.*[a-z])(?=.*[A-Z])(?=.*\\d)(?=.*[@$!%*?&]).{6,}$')
      ]],
      confirmPassword: ['', [Validators.required]],
      name: ['', [Validators.required, Validators.pattern('^[A-Za-z ]+$')]],
      email: ['', [Validators.required, Validators.email]],
      phone: ['', [Validators.required, Validators.pattern('^[0-9]{10}$')]]
    }, {
      validators: this.passwordsMatchValidator()
    });
  }
  showPassword = false;
loading = false;

  get f() { return this.form.controls; }

  submit() {
  if (this.form.invalid){ 
    return;}

  this.loading = true;
  const { confirmPassword, ...payload } = this.form.value;

  this.auth.register(payload).subscribe({
    next: () => {
      this.loading = false;
      this.toastr.success('Registration successful ✅');
      this.router.navigate(['/login']);
    },
    error: (err) => {
      this.loading = false;
      this.toastr.error(err.error?.message || 'Registration failed ❌');
    }
  });
}
togglePassword() {
  this.showPassword = !this.showPassword;
}
showConfirmPassword = false;
toggleConfirmPassword() {
  this.showConfirmPassword = !this.showConfirmPassword;
}

passwordStrengthClass() {
  const value = this.form.get('password')?.value || '';
  const strength = this.getStrengthScore(value);
  if (strength <= 1) return 'bg-red-500 w-1/4';
  if (strength <= 3) return 'bg-yellow-500 w-2/4';
  if (strength === 4) return 'bg-lime-500 w-3/4';
  return 'bg-green-500 w-full';
}

passwordStrengthText() {
  const value = this.form.get('password')?.value || '';
  const strength = this.getStrengthScore(value);
  if (!value) return 'Enter a password';
  if (strength <= 1) return 'Weak';
  if (strength <= 3) return 'Medium';
  if (strength === 4) return 'Strong';
  return 'Very strong';
}

private getStrengthScore(value: string): number {
  let score = 0;
  if (/[A-Z]/.test(value)) score++;
  if (/\d/.test(value)) score++;
  if (/[^A-Za-z0-9]/.test(value)) score++;
  if (value.length >= 6) score++;
  return score;
}

private passwordsMatchValidator(): ValidatorFn {
  return (control: AbstractControl): ValidationErrors | null => {
    const password = control.get('password')?.value;
    const confirmPassword = control.get('confirmPassword')?.value;
    if (!password || !confirmPassword) return null;
    return password === confirmPassword ? null : { passwordMismatch: true };
  };
}
}