import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Router } from '@angular/router';
import { CurrentUserService } from '../services/current-user';
import { UserService } from '../services/user';
import { ToastrService } from 'ngx-toastr';

@Component({
  selector: 'app-admin-profile',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './admin-profile.html'
})
export class AdminProfile implements OnInit {
  user: any = null;
  edit = { name: '', username: '', role: 'Admin', email: '', phone: '' };
  pwd = { oldPassword: '', newPassword: '', confirmPassword: '' };
  showEditPopup = false;
  showPasswordPopup = false;

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
        role: u?.role || 'Admin',
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

  backToAdmin() {
    this.router.navigate(['/admin']);
  }
}
