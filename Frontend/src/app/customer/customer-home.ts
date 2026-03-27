import { Component, OnInit, ChangeDetectionStrategy, ChangeDetectorRef, HostListener } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router, RouterModule } from '@angular/router';
import { MovieService } from '../services/movie';
import { CurrentUserService } from '../services/current-user';
import { UserService } from '../services/user';
import { PreferencesSetup } from './preferences-setup';
import { finalize } from 'rxjs/operators';
import { catchError, of, timeout } from 'rxjs';

@Component({
  selector: 'app-customer-home',
  standalone: true,
  imports: [CommonModule, RouterModule, PreferencesSetup],
  templateUrl: './customer-home.html',
  changeDetection: ChangeDetectionStrategy.Default
})
export class CustomerHome implements OnInit {
  newMovies: any[] = [];
  topRatedMovies: any[] = [];
  topRentedMovies: any[] = [];
  suggestedMovies: any[] = [];
  hasSuggestions = false;
  loading = true;

  showPreferencesPopup = false;
  userId = 0;

  // Carousel offsets per section
  offsets: Record<string, number> = {
    suggested: 0,
    newMovies: 0,
    topRated: 0,
    topRented: 0
  };

  visibleCount = 5;

  constructor(
    private movieService: MovieService,
    private currentUser: CurrentUserService,
    private userService: UserService,
    private router: Router,
    private cdr: ChangeDetectorRef
  ) {}

  @HostListener('window:resize')
  onResize() {
    const w = window.innerWidth;
    this.visibleCount = w < 640 ? 1 : w < 1024 ? 2 : 3;
    this.cdr.detectChanges();
  }

  ngOnInit(): void {
    this.onResize();

    setTimeout(() => {
      if (this.loading) { this.loading = false; this.cdr.markForCheck(); }
    }, 5000);

    let pending = 3;
    const done = () => {
      pending--;
      if (pending <= 0) {
        setTimeout(() => { this.loading = false; this.cdr.markForCheck(); }, 0);
      }
    };

    this.movieService.getAll().pipe(
      timeout(10000), catchError(() => of([])), finalize(done)
    ).subscribe({
      next: (movies) => {
        const sortedByAdded = [...(movies || [])].sort((a, b) => (b.id || 0) - (a.id || 0));
        this.newMovies = sortedByAdded.slice(0, 10);
      }
    });

    this.movieService.getTopUserRated(10).pipe(
      timeout(10000), catchError(() => of([])), finalize(done)
    ).subscribe({ next: (res) => (this.topRatedMovies = res || []) });

    this.movieService.getTopRented(10).pipe(
      timeout(10000), catchError(() => of([])), finalize(done)
    ).subscribe({ next: (res) => (this.topRentedMovies = res || []) });

    this.currentUser.loadCurrentUser().subscribe(user => {
      this.userId = user?.id ?? this.currentUser.decodedUserId;
      if (this.userId > 0) {
        this.userService.getPreferences(this.userId).pipe(catchError(() => of(null))).subscribe(pref => {
          if (!pref || !pref.isSet) {
            this.showPreferencesPopup = true;
          } else {
            this.loadSuggestions();
          }
          this.cdr.detectChanges();
        });
      }
    });
  }

  loadSuggestions() {
    if (this.userId > 0) {
      this.movieService.getSuggestions(this.userId).pipe(catchError(() => of([]))).subscribe(res => {
        this.suggestedMovies = res || [];
        this.hasSuggestions = this.suggestedMovies.length > 0;
        this.cdr.detectChanges();
      });
    }
  }

  onPreferencesDone() {
    this.showPreferencesPopup = false;
    this.loadSuggestions();
    this.cdr.detectChanges();
  }

  // Returns translateX percentage for a section's track
  trackTranslate(key: string): string {
    const offset = this.offsets[key] ?? 0;
    // Each card is (100 / visibleCount)% wide, gap is handled via padding trick
    return `translateX(calc(-${offset} * (100% / ${this.visibleCount}) - ${offset} * 1rem))`;
  }

  prev(key: string) {
    if ((this.offsets[key] ?? 0) > 0) {
      this.offsets[key]--;
      this.cdr.detectChanges();
    }
  }

  next(key: string, list: any[]) {
    if ((this.offsets[key] ?? 0) + this.visibleCount < list.length) {
      this.offsets[key]++;
      this.cdr.detectChanges();
    }
  }

  canPrev(key: string): boolean {
    return (this.offsets[key] ?? 0) > 0;
  }

  canNext(key: string, list: any[]): boolean {
    return (this.offsets[key] ?? 0) + this.visibleCount < list.length;
  }

  openMovie(movieId: number) {
    this.router.navigate(['/dashboard/movie', movieId]);
  }
}

