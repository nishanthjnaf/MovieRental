import { ChangeDetectorRef, Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ActivatedRoute, Router } from '@angular/router';
import { FormsModule } from '@angular/forms';
import { DomSanitizer, SafeResourceUrl } from '@angular/platform-browser';
import { SeriesService } from '../services/series';
import { CurrentUserService } from '../services/current-user';
import { CartStateService } from '../services/cart-state';
import { ToastrService } from 'ngx-toastr';
import { catchError, of } from 'rxjs';

@Component({
  selector: 'app-customer-series-detail',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './customer-series-detail.html'
})
export class CustomerSeriesDetail implements OnInit {
  series: any = null;
  loading = true;
  isAlreadyRented = false;
  rentalEndDate: string | null = null;
  isInWatchlist = false;
  watchlistItemId: number | null = null;
  existingReviews: Record<number, any> = {};
  private seriesId = 0;
  userId = 0;

  expandedSeasonId: number | null = null;

  reviewSeasonId: number | null = null;
  reviewRating = 5;
  reviewComment = '';

  constructor(
    private route: ActivatedRoute,
    private router: Router,
    private seriesService: SeriesService,
    private currentUser: CurrentUserService,
    private cart: CartStateService,
    private toastr: ToastrService,
    private cdr: ChangeDetectorRef,
    private sanitizer: DomSanitizer
  ) {}

  get trailerEmbedUrl(): SafeResourceUrl {
    const url = this.series?.trailerUrl;
    let embedUrl = 'https://www.youtube.com/embed/dQw4w9WgXcQ';
    if (url) {
      const match = url.match(/(?:youtu\.be\/|youtube\.com\/(?:watch\?v=|embed\/|v\/))([A-Za-z0-9_-]{11})/);
      if (match?.[1]) embedUrl = `https://www.youtube.com/embed/${match[1]}`;
    }
    return this.sanitizer.bypassSecurityTrustResourceUrl(embedUrl);
  }

  get rentalCountdown(): string | null {
    if (!this.rentalEndDate) return null;
    const diff = new Date(this.rentalEndDate).getTime() - Date.now();
    if (diff <= 0) return 'Expired';
    const hours = diff / (1000 * 60 * 60);
    if (hours < 24) return `${Math.ceil(hours)}h left`;
    const days = Math.ceil(hours / 24);
    return `${days} day${days === 1 ? '' : 's'} left`;
  }

  ngOnInit(): void {
    this.route.paramMap.subscribe(params => {
      this.seriesId = Number(params.get('id'));
      if (!this.seriesId) return;
      this.load();
    });
  }

  load() {
    this.loading = true;
    this.userId = this.currentUser.currentUserId || this.currentUser.decodedUserId;

    this.seriesService.getById(this.seriesId).pipe(catchError(() => of(null))).subscribe(s => {
      this.series = s;
      this.loading = false;
      this.cdr.detectChanges();
    });

    if (this.userId > 0) {
      this.seriesService.getRentalsByUser(this.userId).pipe(catchError(() => of([]))).subscribe((rentals: any[]) => {
        const active = (rentals || []).find(r => r.seriesId === this.seriesId && r.isActive);
        this.isAlreadyRented = !!active;
        this.rentalEndDate = active?.endDate ?? null;
        this.cdr.detectChanges();
      });

      this.seriesService.getWatchlistByUser(this.userId).pipe(catchError(() => of([]))).subscribe((list: any[]) => {
        const found = (list || []).find(w => w.seriesId === this.seriesId);
        this.isInWatchlist = !!found;
        this.watchlistItemId = found?.id ?? null;
        this.cdr.detectChanges();
      });

      this.seriesService.getReviewsByUser(this.userId).pipe(catchError(() => of([]))).subscribe((reviews: any[]) => {
        this.existingReviews = {};
        (reviews || []).forEach(r => { this.existingReviews[r.seasonId] = r; });
        this.cdr.detectChanges();
      });
    }
  }

  toggleSeason(seasonId: number) {
    this.expandedSeasonId = this.expandedSeasonId === seasonId ? null : seasonId;
    this.cdr.detectChanges();
  }

  watchSeries() {
    this.router.navigate(['/dashboard/watch-series', this.seriesId]);
  }

  addToCart() {
    if (!this.userId) { this.toastr.error('Please login'); return; }
    const result = this.cart.addSeries(this.series);
    if (!result) return;
    result.subscribe({
      next: (res) => {
        if (res === null) this.toastr.error('Could not add to cart');
        else if (res === 'exists') this.toastr.info('Already in cart');
        else this.toastr.success('Added to cart');
        this.cdr.detectChanges();
      },
      error: () => this.toastr.error('Could not add to cart')
    });
  }

  toggleWatchlist() {
    if (!this.userId) { this.toastr.error('Please login'); return; }
    if (this.isInWatchlist && this.watchlistItemId) {
      this.seriesService.removeFromWatchlist(this.watchlistItemId).subscribe({
        next: () => { this.isInWatchlist = false; this.watchlistItemId = null; this.toastr.success('Removed from watchlist'); this.cdr.detectChanges(); },
        error: () => this.toastr.error('Could not remove from watchlist')
      });
    } else {
      this.seriesService.addToWatchlist(this.userId, this.seriesId).subscribe({
        next: (res: any) => { this.isInWatchlist = true; this.watchlistItemId = res.id; this.toastr.success('Added to watchlist'); this.cdr.detectChanges(); },
        error: (err) => this.toastr.error(err?.error || 'Could not add to watchlist')
      });
    }
  }

  openReviewForm(seasonId: number) {
    this.reviewSeasonId = seasonId;
    const existing = this.existingReviews[seasonId];
    this.reviewRating = existing?.rating ?? 5;
    this.reviewComment = existing?.comment ?? '';
    this.cdr.detectChanges();
  }

  submitReview() {
    if (!this.reviewSeasonId) return;
    this.seriesService.addSeasonReview({
      userId: this.userId,
      seasonId: this.reviewSeasonId,
      rating: this.reviewRating,
      comment: this.reviewComment
    }).subscribe({
      next: (res: any) => {
        this.existingReviews[this.reviewSeasonId!] = res;
        this.toastr.success('Review submitted');
        this.reviewSeasonId = null;
        this.load();
        this.cdr.detectChanges();
      },
      error: (err) => this.toastr.error(err?.error || 'Could not submit review')
    });
  }

  cancelReview() {
    this.reviewSeasonId = null;
    this.cdr.detectChanges();
  }
}
