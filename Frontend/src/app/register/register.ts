import { Component, OnInit } from '@angular/core';
import { FormBuilder, Validators, ReactiveFormsModule } from '@angular/forms';
import { AuthService } from '../services/auth';
import { Router, RouterModule } from '@angular/router';
import { CommonModule } from '@angular/common';
import { ToastrService } from 'ngx-toastr';

@Component({
  selector: 'app-register',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule,RouterModule], // ✅ IMPORTANT
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
      name: ['', [Validators.required, Validators.pattern('^[A-Za-z ]+$')]],
      email: ['', [Validators.required, Validators.email]],
      phone: ['', [Validators.required, Validators.pattern('^[0-9]{10}$')]]
    });
  }
  showPassword = false;
loading = false;

  get f() { return this.form.controls; }

  submit() {
  if (this.form.invalid){ 
    return;}

  this.loading = true;

  this.auth.register(this.form.value).subscribe({
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

passwordStrengthClass() {
  const value = this.form.get('password')?.value || '';

  if (value.length < 4) return 'bg-red-500 w-1/4';
  if (value.length < 6) return 'bg-yellow-500 w-2/4';
  if (value.length >= 6) return 'bg-green-500 w-full';

  return '';
}
}