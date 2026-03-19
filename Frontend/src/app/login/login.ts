import { Component, OnInit } from '@angular/core';
import { FormBuilder, Validators, ReactiveFormsModule } from '@angular/forms';
import { AuthService } from '../services/auth';
import { CommonModule } from '@angular/common';
import { Router, RouterModule } from '@angular/router';
import { ToastrService } from 'ngx-toastr';
import { jwtDecode } from "c:/Users/Nishanth.J/Movie-Rental/node_modules/jwt-decode/build/esm/index"

@Component({
  selector: 'app-login',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule,RouterModule], // ✅ IMPORTANT
  templateUrl: './login.html'
})
export class Login implements OnInit {
  

  form: any;

  constructor(private fb: FormBuilder, private auth: AuthService,private router:Router,private toastr: ToastrService) {}
  showPassword = false;

togglePassword() {
  this.showPassword = !this.showPassword;
}

  ngOnInit() {
    const token =
    localStorage.getItem('token') ||
    sessionStorage.getItem('token');
    const role = localStorage.getItem('role');

    if (token && role) {
      if (role === 'Admin') {
        this.router.navigate(['/admin']);
      } else {
        this.router.navigate(['/dashboard']);
      }
    }
    this.form = this.fb.group({
      username: ['', Validators.required],
      password: ['', Validators.required],
      remember: [false]
    });
  }

  get f() { return this.form.controls; }

  submit() {
    if (this.form.invalid){ 
      return;}

    this.auth.login(this.form.value).subscribe({
      next: (res: any) => {
        this.toastr.success('Login successful');
        const token = res.token;
        
        if (this.form.value.remember) {
          localStorage.setItem('token', token); // permanent
        } else {
          sessionStorage.setItem('token', token); // temporary
        }
        const decoded: any = jwtDecode(token);
        const role = decoded.role || decoded.Role || decoded["http://schemas.microsoft.com/ws/2008/06/identity/claims/role"];
        localStorage.setItem('role', role);
        if (role === 'Admin') {
        this.router.navigate(['/admin']);
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