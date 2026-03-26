import { Component } from '@angular/core';
import { Router } from '@angular/router';

@Component({
  selector: 'app-not-found',
  standalone: true,
  template: `
    <div class="min-h-screen bg-slate-950 text-slate-100 flex flex-col items-center justify-center gap-6">
      <p class="text-8xl font-bold text-amber-300">404</p>
      <p class="text-2xl text-slate-300">Page not found</p>
      <p class="text-slate-500 text-sm">The page you're looking for doesn't exist.</p>
      <button class="bg-amber-600 hover:bg-amber-500 rounded-full px-6 py-2 mt-2" (click)="go()">
        Go Home
      </button>
    </div>
  `
})
export class NotFound {
  constructor(private router: Router) {}
  go() { this.router.navigate(['/']); }
}
