import { ChangeDetectorRef, Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router } from '@angular/router';
import { CurrentUserService } from '../services/current-user';
import { PreferencesSetup } from './preferences-setup';

@Component({
  selector: 'app-customer-preferences-page',
  standalone: true,
  imports: [CommonModule, PreferencesSetup],
  template: `
    <div class="min-h-screen bg-slate-950 flex items-center justify-center">
      <app-preferences-setup
        *ngIf="userId > 0"
        [userId]="userId"
        (done)="onDone()">
      </app-preferences-setup>
    </div>
  `
})
export class CustomerPreferencesPage implements OnInit {
  userId = 0;

  constructor(private currentUser: CurrentUserService, private router: Router, private cdr: ChangeDetectorRef) {}

  ngOnInit() {
    this.currentUser.loadCurrentUser().subscribe(user => {
      this.userId = user?.id ?? this.currentUser.decodedUserId;
      this.cdr.detectChanges();
    });
  }

  onDone() {
    this.router.navigate(['/dashboard/home']);
  }}
