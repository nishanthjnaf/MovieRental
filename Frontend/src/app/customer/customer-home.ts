import { Component, OnInit, OnDestroy, ChangeDetectionStrategy, ChangeDetectorRef, HostListener } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router, RouterModule } from '@angular/router';
import { MovieService } from '../services/movie';
import { SeriesService } from '../services/series';
import { CurrentUserService } from '../services/current-user';
import { UserService } from '../services/user';
import { InventoryService } from '../services/inventory';
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
export class CustomerHome implements OnInit, OnDestroy {
  newMovies: any[] = [];
  topRatedMovies: any[] = [];
  topRentedMovies: any[] = [];
  suggestedMovies: any[] = [];
  hasSuggestions = false;
  loading = true;

  // Series sections
  newSeries: any[] = [];
  topRatedMixed: any[] = [];
  topRentedMixed: any[] = [];
  suggestedMixed: any[] = [];

  showPreferencesPopup = false;
  userId = 0;

  // Hero slideshow
  heroMovies: any[] = [];
  heroIndex = 0;
  private heroTimer: any;
  rentedMovieIds = new Set<number>();

  // Carousel offsets per section
  offsets: Record<string, number> = {
    suggested: 0,
    newMovies: 0,
    newSeries: 0,
    topRated: 0,
    topRented: 0
  };

  visibleCount = 5;

  constructor(
    private movieService: MovieService,
    private seriesService: SeriesService,
    private currentUser: CurrentUserService,
    private userService: UserService,
    private inventoryService: InventoryService,
    private router: Router,
    private cdr: ChangeDetectorRef
  ) {}

  private availableMovieIds = new Set<number>();

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

    // Load inventory availability first, then movies
    this.inventoryService.getAll().pipe(
      timeout(8000), catchError(() => of([]))
    ).subscribe((rows: any[]) => {
      (rows || []).forEach((r: any) => {
        if (r?.isAvailable && r?.movieId) this.availableMovieIds.add(Number(r.movieId));
      });
      this.loadMovieSections();
    });

    this.currentUser.loadCurrentUser().subscribe(user => {
      this.userId = user?.id ?? this.currentUser.decodedUserId;
      if (this.userId > 0) {
        this.userService.getRentedMovies(this.userId).pipe(catchError(() => of([]))).subscribe((items: any[]) => {
          this.rentedMovieIds = new Set((items || []).filter((i: any) => i.isActive).map((i: any) => i.movieId));
          this.cdr.detectChanges();
        });
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

  private loadMovieSections() {
    let pending = 5;
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
        const available = (movies || []).filter(m => this.availableMovieIds.has(m.id));
        const sortedByAdded = [...available].sort((a, b) => (b.id || 0) - (a.id || 0));
        this.newMovies = sortedByAdded.slice(0, 10);
        this.heroMovies = sortedByAdded.slice(0, 3);
        this.startHeroTimer();
      }
    });

    this.movieService.getTopUserRated(10).pipe(
      timeout(10000), catchError(() => of([])), finalize(done)
    ).subscribe({
      next: (res) => {
        const movies = (res || []).filter((m: any) => this.availableMovieIds.has(m.id)).map((m: any) => ({ ...m, _type: 'movie' }));
        this.seriesService.getTopRated(10).pipe(catchError(() => of([]))).subscribe(series => {
          const s = (series || []).map((s: any) => ({ ...s, _type: 'series' }));
          this.topRatedMixed = [...movies, ...s].sort((a, b) => (b.rating || this.seriesAvgRating(b)) - (a.rating || this.seriesAvgRating(a))).slice(0, 10);
          this.cdr.detectChanges();
        });
      }
    });

    this.movieService.getTopRented(10).pipe(
      timeout(10000), catchError(() => of([])), finalize(done)
    ).subscribe({
      next: (res) => {
        const movies = (res || []).filter((m: any) => this.availableMovieIds.has(m.movieId ?? m.id)).map((m: any) => ({ ...m, _type: 'movie' }));
        this.seriesService.getTopRented(10).pipe(catchError(() => of([]))).subscribe(series => {
          const s = (series || []).map((s: any) => ({ ...s, _type: 'series' }));
          this.topRentedMixed = [...movies, ...s].sort((a, b) => (b.rentalCount || 0) - (a.rentalCount || 0)).slice(0, 10);
          this.cdr.detectChanges();
        });
      }
    });

    this.seriesService.getNew(10).pipe(
      timeout(10000), catchError(() => of([])), finalize(done)
    ).subscribe({
      next: (res) => { this.newSeries = res || []; }
    });

    // dummy 5th pending resolver
    Promise.resolve().then(done);
  }

  ngOnDestroy() {
    this.stopHeroTimer();
  }

  private startHeroTimer() {
    this.stopHeroTimer();
    this.heroTimer = setInterval(() => {
      this.heroIndex = (this.heroIndex + 1) % Math.max(1, this.heroMovies.length);
      this.cdr.detectChanges();
    }, 5000);
  }

  private stopHeroTimer() {
    if (this.heroTimer) { clearInterval(this.heroTimer); this.heroTimer = null; }
  }

  setHeroIndex(i: number) {
    this.heroIndex = i;
    this.startHeroTimer(); // reset timer on manual click
    this.cdr.detectChanges();
  }

  isRented(movieId: number): boolean {
    return this.rentedMovieIds.has(movieId);
  }

  heroAction(movie: any) {
    if (this.isRented(movie.id)) {
      this.router.navigate(['/dashboard/watch', movie.id]);
    } else {
      this.router.navigate(['/dashboard/movie', movie.id]);
    }
  }

  loadSuggestions() {
    if (this.userId > 0) {
      this.movieService.getSuggestions(this.userId).pipe(catchError(() => of([]))).subscribe(res => {
        const movies = (res || []).map((m: any) => ({ ...m, _type: 'movie' }));
        this.seriesService.getSuggestions(this.userId).pipe(catchError(() => of([]))).subscribe(series => {
          const s = (series || []).map((s: any) => ({ ...s, _type: 'series' }));
          this.suggestedMixed = [...movies, ...s];
          this.hasSuggestions = this.suggestedMixed.length > 0;
          this.cdr.detectChanges();
        });
      });
    }
  }

  private seriesAvgRating(s: any): number {
    if (!s.seasons?.length) return 0;
    const ratings = s.seasons.filter((sn: any) => sn.averageRating > 0).map((sn: any) => sn.averageRating);
    return ratings.length ? ratings.reduce((a: number, b: number) => a + b, 0) / ratings.length : 0;
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

  openItem(item: any) {
    if (item._type === 'series') {
      this.router.navigate(['/dashboard/series', item.id]);
    } else {
      this.router.navigate(['/dashboard/movie', item.movieId || item.id]);
    }
  }

  getItemRating(item: any): number {
    if (item._type === 'series') return this.seriesAvgRating(item);
    return item.rating || 0;
  }
}

