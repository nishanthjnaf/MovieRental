import { ChangeDetectorRef, Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ActivatedRoute, Router } from '@angular/router';
import { FormsModule } from '@angular/forms';
import { DomSanitizer, SafeResourceUrl } from '@angular/platform-browser';
import { MovieService } from '../services/movie';
import { CurrentUserService } from '../services/current-user';
import { UserService } from '../services/user';
import { WatchlistService } from '../services/watchlist';
import { CartStateService } from '../services/cart-state';
import { RentalService } from '../services/rental';
import { PaymentService } from '../services/payment';
import { ToastrService } from 'ngx-toastr';
import { ReviewService } from '../services/review';
import { InventoryService } from '../services/inventory';
import { finalize } from 'rxjs/operators';
import { catchError, of, timeout } from 'rxjs';

@Component({
  selector: 'app-customer-movie-detail',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './customer-movie-detail.html'
})
export class CustomerMovieDetail implements OnInit {
  movie: any = null;
  isAlreadyRented = false;
  isAvailableToRent = true;
  rentalPrice: number | null = null;
  loading = true;
  rating = 5;
  comment = '';
  genreNames: string[] = [];
  isInWatchlist = false;
  existingReview: any = null;   // null = not rated yet
  private movieId = 0;

  constructor(
    private route: ActivatedRoute,
    private router: Router,
    private movieService: MovieService,
    private currentUser: CurrentUserService,
    private userService: UserService,
    private watchlistService: WatchlistService,
    private cartState: CartStateService,
    private rentalService: RentalService,
    private paymentService: PaymentService,
    private reviewService: ReviewService,
    private inventoryService: InventoryService,
    private toastr: ToastrService,
    private cdr: ChangeDetectorRef,
    private sanitizer: DomSanitizer
  ) {}

  get trailerEmbedUrl(): SafeResourceUrl {
    const url = this.movie?.trailerUrl;
    let embedUrl = 'https://www.youtube.com/embed/dQw4w9WgXcQ'; // fallback dummy
    if (url) {
      const match = url.match(/(?:youtu\.be\/|youtube\.com\/(?:watch\?v=|embed\/|v\/))([A-Za-z0-9_-]{11})/);
      if (match?.[1]) embedUrl = `https://www.youtube.com/embed/${match[1]}`;
    }
    return this.sanitizer.bypassSecurityTrustResourceUrl(embedUrl);
  }

  ngOnInit(): void {
    this.route.paramMap.subscribe((params) => {
      this.movieId = Number(params.get('id'));
      if (!this.movieId) return;
      this.loading = true;
      this.isAlreadyRented = false;
      this.isInWatchlist = false;

      const userId = this.currentUser.currentUserId || this.currentUser.decodedUserId;
      let pending = userId ? 3 : 2;

      const done = () => {
        pending--;
        if (pending <= 0) {
          this.loading = false;
          this.cdr.detectChanges();
        }
      };

      this.movieService.getById(this.movieId).pipe(
        timeout(10000), catchError(() => of(null)), finalize(done)
      ).subscribe({
        next: (m) => {
          this.movie = m;
          const genres = Array.isArray(m?.genres) ? m.genres : [];
          this.genreNames = genres.map((g: any) => (typeof g === 'string' ? g : g?.name)).filter((g: any) => !!g);
          // Check watchlist
          if (userId) {
            this.watchlistService.getByUser(userId).pipe(catchError(() => of([]))).subscribe((list: any[]) => {
              this.isInWatchlist = (list || []).some((w: any) => w.movieId === this.movieId);
              this.cdr.detectChanges();
            });
          }
        }
      });

      this.inventoryService.getByMovie(this.movieId).pipe(
        timeout(10000), catchError(() => of([])), finalize(done)
      ).subscribe({
        next: (inv) => {
          const list = Array.isArray(inv) ? inv : [inv];
          this.isAvailableToRent = list.some((i: any) => i?.isAvailable);
          const firstAvailable = list.find((i: any) => i?.isAvailable);
          const rawPrice = firstAvailable?.rentalPrice ?? list[0]?.rentalPrice;
          const parsed = Number(rawPrice);
          this.rentalPrice = Number.isFinite(parsed) ? parsed : null;
        },
        error: () => { this.isAvailableToRent = false; this.rentalPrice = null; }
      });

      if (userId) {
        this.userService.getRentedMovies(userId).pipe(
          catchError(() => of([])), finalize(done)
        ).subscribe({
          next: (items) => {
            this.isAlreadyRented = (items || []).some((i: any) => i.movieId === this.movieId && i.isActive);
            // Check existing review for this movie
            this.reviewService.getByUser(userId).pipe(catchError(() => of([]))).subscribe((reviews: any[]) => {
              const found = (reviews || []).find((r: any) => r.movieId === this.movieId);
              if (found) {
                this.existingReview = found;
                this.rating = found.rating ?? 5;
                this.comment = found.comment ?? '';
              }
              this.cdr.detectChanges();
            });
          }
        });
      }
    });
  }

  toggleWatchlist() {
    const userId = this.currentUser.currentUserId || this.currentUser.decodedUserId;
    if (!userId || !this.movie?.id) return;
    if (this.isInWatchlist) {
      // Find the watchlist item id and remove
      this.watchlistService.getByUser(userId).pipe(catchError(() => of([]))).subscribe((list: any[]) => {
        const item = (list || []).find((w: any) => w.movieId === this.movie.id);
        if (item) {
          this.watchlistService.remove(item.id).subscribe({
            next: () => { this.isInWatchlist = false; this.toastr.success('Removed from watchlist'); this.cdr.detectChanges(); },
            error: () => this.toastr.error('Could not remove')
          });
        }
      });
    } else {
      this.watchlistService.add(userId, this.movie.id).subscribe({
        next: () => { this.isInWatchlist = true; this.toastr.success('Added to watchlist'); this.cdr.detectChanges(); },
        error: () => this.toastr.info('Already in watchlist')
      });
    }
  }

  addToCart() {
    if (!this.movie || this.isAlreadyRented) return;
    const result = this.cartState.add(this.movie);
    if (!result) return;
    result.subscribe({
      next: (res) => {
        if (res === null) {
          this.toastr.error('Could not add to cart');
        } else if (res === 'exists') {
          this.toastr.info('Already in cart');
        } else {
          // reload() is called inside add() via tap — just show success
          this.toastr.success('Added to cart');
        }
      },
      error: () => this.toastr.error('Could not add to cart')
    });
  }

  rentNow() {
    const userId = this.currentUser.currentUserId || this.currentUser.decodedUserId;
    if (!userId || !this.movie?.id || !this.isAvailableToRent) return;
    this.rentalService.createRental({ userId, movieIds: [this.movie.id], rentalDays: 7 }).subscribe({
      next: (rental) => {
        this.paymentService.makePayment({ rentalId: rental.id, method: 1, isSuccess: true }).subscribe({
          next: () => {
            this.toastr.success('Rental successful');
            this.isAlreadyRented = true;
          },
          error: () => this.toastr.error('Payment failed')
        });
      },
      error: () => this.toastr.error('Could not create rental')
    });
  }

  watchMovie() {
    if (this.movie?.id) this.router.navigate(['/dashboard/watch', this.movie.id]);
  }

  submitReview() {
    const userId = this.currentUser.currentUserId || this.currentUser.decodedUserId;
    if (!userId || !this.movie?.id || !this.isAlreadyRented) return;
    this.reviewService.addReview({
      userId,
      movieId: this.movie.id,
      rating: this.rating,
      comment: this.comment || ''
    }).subscribe({
      next: (res: any) => {
        this.existingReview = { rating: this.rating, comment: this.comment, ...(res || {}) };
        this.toastr.success('Review submitted');
        this.cdr.detectChanges();
      },
      error: () => {
        this.toastr.info('You already reviewed this movie');
        this.existingReview = { rating: this.rating, comment: this.comment };
        this.cdr.detectChanges();
      }
    });
  }
}

