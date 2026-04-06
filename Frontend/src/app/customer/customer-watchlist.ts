import { ChangeDetectorRef, Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router } from '@angular/router';
import { WatchlistService } from '../services/watchlist';
import { SeriesService } from '../services/series';
import { CurrentUserService } from '../services/current-user';
import { MovieService } from '../services/movie';
import { UserService } from '../services/user';
import { ToastrService } from 'ngx-toastr';
import { CartStateService } from '../services/cart-state';
import { catchError, of } from 'rxjs';

@Component({
  selector: 'app-customer-watchlist',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './customer-watchlist.html'
})
export class CustomerWatchlist implements OnInit {
  items: any[] = [];
  loading = true;
  showConfirmPopup = false;
  pendingRemoveId: number | null = null;
  pendingRemoveIsSeries = false;
  rentedMovieIds = new Set<number>();
  rentedSeriesIds = new Set<number>();

  // Filter: 'all' | 'movies' | 'series'
  watchlistFilter: 'all' | 'movies' | 'series' = 'all';

  get filteredItems(): any[] {
    if (this.watchlistFilter === 'movies') return this.items.filter(i => !i._isSeries);
    if (this.watchlistFilter === 'series') return this.items.filter(i => i._isSeries);
    return this.items;
  }

  constructor(
    private watchlistService: WatchlistService,
    private seriesService: SeriesService,
    private currentUser: CurrentUserService,
    private movieService: MovieService,
    private userService: UserService,
    private cart: CartStateService,
    private router: Router,
    private toastr: ToastrService,
    private cdr: ChangeDetectorRef
  ) {}

  ngOnInit(): void {
    this.load();
  }

  load() {
    const userId = this.currentUser.currentUserId || this.currentUser.decodedUserId;
    if (!userId) { this.loading = false; this.cdr.detectChanges(); return; }

    // Load rented IDs
    this.userService.getRentedMovies(userId).pipe(catchError(() => of([]))).subscribe((items: any[]) => {
      this.rentedMovieIds = new Set((items || []).filter((i: any) => i.isActive).map((i: any) => i.movieId));
      this.cdr.detectChanges();
    });
    this.seriesService.getRentalsByUser(userId).pipe(catchError(() => of([]))).subscribe((items: any[]) => {
      this.rentedSeriesIds = new Set((items || []).filter((i: any) => i.isActive).map((i: any) => i.seriesId));
      this.cdr.detectChanges();
    });

    let pending = 2;
    const done = () => { pending--; if (pending <= 0) { this.loading = false; this.cdr.detectChanges(); } };

    // Movie watchlist
    this.watchlistService.getByUser(userId).pipe(catchError(() => of([]))).subscribe({
      next: (list) => {
        const src = list || [];
        if (!src.length) { done(); return; }
        let p = src.length;
        src.forEach((i: any) => {
          this.movieService.getById(i.movieId).subscribe({
            next: (movie) => {
              this.items = [...this.items.filter(x => x.id !== i.id || x._isSeries), { ...i, movie, _isSeries: false }];
              p--; if (p <= 0) done();
            },
            error: () => { p--; if (p <= 0) done(); }
          });
        });
      },
      error: () => done()
    });

    // Series watchlist
    this.seriesService.getWatchlistByUser(userId).pipe(catchError(() => of([]))).subscribe({
      next: (list) => {
        const src = list || [];
        if (!src.length) { done(); return; }
        let p = src.length;
        src.forEach((i: any) => {
          this.seriesService.getById(i.seriesId).subscribe({
            next: (series) => {
              this.items = [...this.items.filter(x => !(x._isSeries && x.id === i.id)), { ...i, movie: series, movieId: i.seriesId, _isSeries: true }];
              p--; if (p <= 0) done();
            },
            error: () => { p--; if (p <= 0) done(); }
          });
        });
      },
      error: () => done()
    });
  }

  isRented(item: any): boolean {
    if (item._isSeries) return this.rentedSeriesIds.has(item.movieId || item.seriesId);
    return this.rentedMovieIds.has(item.movieId);
  }

  open(item: any) {
    if (item._isSeries) this.router.navigate(['/dashboard/series', item.movieId || item.seriesId]);
    else this.router.navigate(['/dashboard/movie', item.movieId]);
  }

  watchMovie(item: any) {
    if (item._isSeries) this.router.navigate(['/dashboard/watch-series', item.movieId || item.seriesId]);
    else this.router.navigate(['/dashboard/watch', item.movieId]);
  }

  addToCart(item: any) {
    if (item._isSeries) { this.router.navigate(['/dashboard/series', item.movieId]); return; }
    const movie = item?.movie;
    if (!movie?.id) return;
    const result = this.cart.add(movie);
    if (!result) return;
    result.subscribe({
      next: (res) => {
        if (res === null) this.toastr.error('Could not add to cart');
        else if (res === 'exists') this.toastr.info('Already in cart');
        else this.toastr.success('Added to cart');
      },
      error: () => this.toastr.error('Could not add to cart')
    });
  }

  remove(itemId: number, isSeries: boolean) {
    this.pendingRemoveId = itemId;
    this.pendingRemoveIsSeries = isSeries;
    this.showConfirmPopup = true;
    this.cdr.detectChanges();
  }

  confirmRemove() {
    if (this.pendingRemoveId === null) return;
    const id = this.pendingRemoveId;
    const isSeries = this.pendingRemoveIsSeries;
    this.items = this.items.filter(i => !(i.id === id && i._isSeries === isSeries));
    this.cancelRemove();
    const obs = isSeries ? this.seriesService.removeFromWatchlist(id) : this.watchlistService.remove(id);
    obs.subscribe({
      next: () => this.toastr.success('Removed from watchlist'),
      error: () => this.toastr.error('Could not remove from watchlist')
    });
    this.cdr.detectChanges();
  }

  cancelRemove() {
    this.showConfirmPopup = false;
    this.pendingRemoveId = null;
    this.cdr.detectChanges();
  }
}

