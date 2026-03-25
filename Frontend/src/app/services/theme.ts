import { Injectable } from '@angular/core';

@Injectable({ providedIn: 'root' })
export class ThemeService {
  private readonly KEY = 'cinefilia_theme';

  get isDark(): boolean {
    return document.documentElement.classList.contains('dark-theme');
  }

  init() {
    const saved = localStorage.getItem(this.KEY) ?? 'dark';
    this.apply(saved === 'dark');
  }

  toggle() {
    this.apply(!this.isDark);
  }

  private apply(dark: boolean) {
    const html = document.documentElement;
    if (dark) {
      html.classList.add('dark-theme');
      html.classList.remove('light-theme');
    } else {
      html.classList.add('light-theme');
      html.classList.remove('dark-theme');
    }
    localStorage.setItem(this.KEY, dark ? 'dark' : 'light');
  }
}
