import { ChangeDetectorRef, Component, OnInit } from '@angular/core';
import { CommonModule, DatePipe } from '@angular/common';
import { CurrentUserService } from '../services/current-user';
import { ReviewService } from '../services/review';
import { SeriesService } from '../services/series';
import { MovieService } from '../services/movie';
import { ToastrService } from 'ngx-toastr';
import { Router } from '@angular/router';
import { catchError, of } from 'rxjs';

@Component({
  selector: 'app-customer-my-ratings',
  standalone: true,
  imports: [CommonModule, DatePipe],
  templateUrl: './customer-my-ratings.html'
})
export class CustomerMyRatings implements OnInit {
  ratings: any[] = [];
  loading = true;
  showConfirmPopup = false;
  pendingDeleteId: number | null = null;
  pendingDeleteIsSeries = false;

  // Filter: 'all' | 'movies' | 'series'
  ratingsFilter: 'all' | 'movies' | 'series' = 'all';

  get filteredRatings(): any[] {
    if (this.ratingsFilter === 'movies') return this.ratings.filter(r => !r._isSeries);
    if (this.ratingsFilter === 'series') return this.ratings.filter(r => r._isSeries);
    return this.ratings;
  }

  constructor(
    private currentUser: CurrentUserService,
    private reviewService: ReviewService,
    private seriesService: SeriesService,
    private movieService: MovieService,
    private toastr: ToastrService,
    private router: Router,
    private cdr: ChangeDetectorRef
  ) {}

  ngOnInit(): void {
    const userId = this.currentUser.currentUserId || this.currentUser.decodedUserId;
    if (!userId) { this.loading = false; this.cdr.detectChanges(); return; }

    let pending = 2;
    const done = () => { pending--; if (pending <= 0) { this.loading = false; this.cdr.detectChanges(); } };

    this.reviewService.getByUser(userId).subscribe({
      next: (res) => {
        const list = res || [];
        if (!list.length) { done(); return; }
        let p = list.length;
        list.forEach((r: any) => {
          this.movieService.getById(r.movieId).pipe(catchError(() => of(null))).subscribe(movie => {
            this.ratings = [...this.ratings.filter(x => x.id !== r.id || x._isSeries), { ...r, movie, _isSeries: false }];
            p--; if (p <= 0) done();
          });
        });
      },
      error: () => done()
    });

    this.seriesService.getReviewsByUser(userId).subscribe({
      next: (res) => {
        const list = res || [];
        if (!list.length) { done(); return; }
        list.forEach((r: any) => {
          this.ratings = [...this.ratings.filter(x => !(x._isSeries && x.id === r.id)), {
            ...r,
            movie: { title: r.seriesTitle },
            _isSeries: true,
            _displayTitle: `${r.seriesTitle}: Season ${r.seasonNumber}`
          }];
        });
        done();
      },
      error: () => done()
    });
  }

  removeRating(id: number, isSeries: boolean) {
    this.pendingDeleteId = id;
    this.pendingDeleteIsSeries = isSeries;
    this.showConfirmPopup = true;
    this.cdr.detectChanges();
  }

  confirmDelete() {
    if (this.pendingDeleteId === null) return;
    const obs = this.pendingDeleteIsSeries
      ? this.seriesService.deleteSeasonReview(this.pendingDeleteId)
      : this.reviewService.deleteReview(this.pendingDeleteId);

    obs.subscribe({
      next: () => {
        this.ratings = this.ratings.filter(r => !(r.id === this.pendingDeleteId && r._isSeries === this.pendingDeleteIsSeries));
        this.toastr.success('Rating removed');
        this.cancelDelete();
      },
      error: () => { this.toastr.error('Could not remove rating'); this.cancelDelete(); }
    });
  }

  cancelDelete() {
    this.showConfirmPopup = false;
    this.pendingDeleteId = null;
    this.cdr.detectChanges();
  }

  openItem(r: any) {
    if (r._isSeries) this.router.navigate(['/dashboard/series', r.seriesId]);
    else this.router.navigate(['/dashboard/movie', r.movieId]);
  }}

