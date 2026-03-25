import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ThemeService } from '../services/theme';

@Component({
  selector: 'app-theme-toggle',
  standalone: true,
  imports: [CommonModule],
  template: `
    <button
      (click)="theme.toggle()"
      class="top-icon-btn"
      [title]="theme.isDark ? 'Switch to Light Mode' : 'Switch to Dark Mode'">
      {{ theme.isDark ? '☀️' : '🌙' }}
    </button>
  `
})
export class ThemeToggle {
  constructor(public theme: ThemeService) {}
}
