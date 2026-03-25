import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { CurrentUserService } from '../services/current-user';
import { UserService } from '../services/user';
import { ToastrService } from 'ngx-toastr';
import { Router } from '@angular/router';
import { PreferencesSetup } from './preferences-setup';

@Component({
  selector: 'app-customer-profile',
  standalone: true,
  imports: [CommonModule, FormsModule, PreferencesSetup],
  templateUrl: './customer-profile.html'
})
export class CustomerProfile implements OnInit {
  user: any = null;
  edit = { name: '', username: '', role: 'Customer', email: '', phone: '' };
  pwd = { oldPassword: '', newPassword: '', confirmPassword: '' };
  showEditPopup = false;
  showPasswordPopup = false;
  showPreferencesPopup = false;

  constructor(
    private currentUser: CurrentUserService,
    private userService: UserService,
    private toastr: ToastrService,
    private router: Router
  ) {}

  ngOnInit(): void {
    this.currentUser.loadCurrentUser().subscribe((u) => {
      this.user = u;
      this.edit = {
        name: u?.name || '',
        username: u?.username || '',
        role: u?.role || 'Customer',
        email: u?.email || '',
        phone: u?.phone || ''
      };
    });
  }

  saveProfile() {
    if (!this.user?.id) return;
    this.userService.updateUser(this.user.id, this.edit).subscribe({
      next: () => {
        this.toastr.success('Profile updated');
        this.showEditPopup = false;
      },
      error: () => this.toastr.error('Could not update profile')
    });
  }

  resetPassword() {
    if (!this.user?.id) return;
    this.userService.resetPassword(this.user.id, this.pwd).subscribe({
      next: () => {
        this.toastr.success('Password changed');
        this.pwd = { oldPassword: '', newPassword: '', confirmPassword: '' };
        this.showPasswordPopup = false;
      },
      error: () => this.toastr.error('Password reset failed')
    });
  }

  deleteAccount() {
    if (!this.user?.id) return;
    if (!confirm('Delete account permanently? This cannot be undone.')) return;
    this.userService.deleteUser(this.user.id).subscribe({
      next: () => {
        this.toastr.success('Account deleted');
        localStorage.removeItem('token');
        sessionStorage.removeItem('token');
        localStorage.removeItem('role');
        this.currentUser.clear();
        this.router.navigate(['/login']);
      },
      error: () => this.toastr.error('Could not delete account')
    });
  }

  onPreferencesSaved() {
    this.showPreferencesPopup = false;
    this.toastr.success('Preferences updated');
  }
}

