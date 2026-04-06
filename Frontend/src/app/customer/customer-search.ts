import { ChangeDetectorRef, Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ActivatedRoute, Router } from '@angular/router';
import { FormsModule } from '@angular/forms';
import { MovieService } from '../services/movie';
import { SeriesService } from '../services/series';
import { catchError, of } from 'rxjs';

@Component({
  selector: 'app-customer-search',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './customer-search.html'
})
export class CustomerSearch implements OnInit {
  movies: any[] = [];
  series: any[] = [];
  q = '';
  loading = true;
  activeTab: 'all' | 'movies' | 'series' = 'all';

  constructor(
    private route: ActivatedRoute,
    private router: Router,
    private movieService: MovieService,
    private seriesService: SeriesService,
    private cdr: ChangeDetectorRef
  ) {}

  ngOnInit(): void {
    this.route.queryParamMap.subscribe(params => {
      this.q = (params.get('q') || '').trim();
      this.search();
    });
  }

  search() {
    this.loading = true;
    this.movies = [];
    this.series = [];

    const q = this.q.toLowerCase();

    let moviesDone = false;
    let seriesDone = false;
    const done = () => {
      if (moviesDone && seriesDone) { this.loading = false; this.cdr.detectChanges(); }
    };

    const movieObs = q
      ? this.movieService.search(q, 1, 50).pipe(catchError(() => of({ items: [] })))
      : this.movieService.getAll().pipe(catchError(() => of([])));

    movieObs.subscribe({
      next: (res: any) => {
        this.movies = (Array.isArray(res) ? res : (res?.items || [])).map((m: any) => ({ ...m, _type: 'movie' }));
        moviesDone = true;
        done();
      },
      error: () => { moviesDone = true; done(); }
    });

    this.seriesService.getAll().pipe(catchError(() => of([]))).subscribe({
      next: (list: any[]) => {
        const all = list || [];
        this.series = (q
          ? all.filter(s => s.title?.toLowerCase().includes(q) || s.director?.toLowerCase().includes(q) || (s.cast || []).join(' ').toLowerCase().includes(q))
          : all
        ).map((s: any) => ({ ...s, _type: 'series' }));
        seriesDone = true;
        done();
      },
      error: () => { seriesDone = true; done(); }
    });
  }

  get allResults(): any[] {
    return [...this.movies, ...this.series];
  }

  get visibleResults(): any[] {
    if (this.activeTab === 'movies') return this.movies;
    if (this.activeTab === 'series') return this.series;
    return this.allResults;
  }

  open(item: any) {
    if (item._type === 'series') this.router.navigate(['/dashboard/series', item.id]);
    else this.router.navigate(['/dashboard/movie', item.id]);
  }

  getAvgRating(item: any): number {
    if (item._type === 'movie') return item.rating || 0;
    if (!item.seasons?.length) return 0;
    const ratings = item.seasons.filter((s: any) => s.averageRating > 0).map((s: any) => s.averageRating);
    return ratings.length ? ratings.reduce((a: number, b: number) => a + b, 0) / ratings.length : 0;
  }
}
