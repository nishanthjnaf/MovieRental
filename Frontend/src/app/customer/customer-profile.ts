import { ChangeDetectorRef, Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { CurrentUserService } from '../services/current-user';
import { UserService } from '../services/user';
import { ToastrService } from 'ngx-toastr';
import { Router } from '@angular/router';
import { PreferencesSetup } from './preferences-setup';
import { ThemeService } from '../services/theme';
import { NotificationService } from '../services/notification';
import { catchError, of } from 'rxjs';

@Component({
  selector: 'app-customer-profile',
  standalone: true,
  imports: [CommonModule, FormsModule, PreferencesSetup],
  templateUrl: './customer-profile.html'
})
export class CustomerProfile implements OnInit {
  user: any = null;
  preferences: any = null;
  edit = { name: '', username: '', role: 'Customer', email: '', phone: '' };
  pwd = { oldPassword: '', newPassword: '', confirmPassword: '' };
  showOldPwd = false;
  showNewPwd = false;

  get newPwdValue(): string { return this.pwd.newPassword || ''; }
  get pwdHasUppercase(): boolean { return /[A-Z]/.test(this.newPwdValue); }
  get pwdHasNumber(): boolean { return /\d/.test(this.newPwdValue); }
  get pwdHasSpecial(): boolean { return /[^A-Za-z0-9]/.test(this.newPwdValue); }
  get pwdHasMinLength(): boolean { return this.newPwdValue.length >= 6; }
  get pwdStrengthClass(): string {
    let s = 0;
    if (this.pwdHasUppercase) s++;
    if (this.pwdHasNumber) s++;
    if (this.pwdHasSpecial) s++;
    if (this.pwdHasMinLength) s++;
    if (s <= 1) return 'auth-strength__fill bg-red-500 w-1/4';
    if (s <= 3) return 'auth-strength__fill bg-yellow-500 w-2/4';
    if (s === 4) return 'auth-strength__fill bg-green-500 w-full';
    return 'auth-strength__fill bg-lime-500 w-3/4';
  }
  get pwdStrengthText(): string {
    if (!this.newPwdValue) return '';
    let s = 0;
    if (this.pwdHasUppercase) s++;
    if (this.pwdHasNumber) s++;
    if (this.pwdHasSpecial) s++;
    if (this.pwdHasMinLength) s++;
    if (s <= 1) return 'Weak';
    if (s <= 3) return 'Medium';
    return 'Strong';
  }
  showEditPopup = false;
  showPasswordPopup = false;
  showPreferencesPopup = false;

  constructor(
    private currentUser: CurrentUserService,
    private userService: UserService,
    private toastr: ToastrService,
    private router: Router,
    public theme: ThemeService,
    private notifService: NotificationService,
    private cdr: ChangeDetectorRef
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
      if (u?.id) {
        this.userService.getPreferences(u.id).pipe(catchError(() => of(null))).subscribe(p => {
          this.preferences = p;
          this.cdr.detectChanges();
        });
      }
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
        this.pwd = { oldPassword: '', newPassword: '', confirmPassword: '' };
        this.showPasswordPopup = false;
        this.cdr.detectChanges();
        this.notifService.refreshUnread(this.user.id);
        this.toastr.success('Password changed');
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
    if (this.user?.id) {
      this.userService.getPreferences(this.user.id).pipe(catchError(() => of(null))).subscribe(p => {
        this.preferences = p;
        this.cdr.detectChanges();
      });
    }
  }
}

