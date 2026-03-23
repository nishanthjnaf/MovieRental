import { Component, OnInit, ChangeDetectorRef } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ActivatedRoute, Router } from '@angular/router';
import { FormsModule } from '@angular/forms';
import { MovieService } from '../services/movie';
import { GenreService } from '../services/genre';
import { InventoryService } from '../services/inventory';
import { catchError, forkJoin, of, timeout } from 'rxjs';

@Component({
  selector: 'app-customer-movies',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './customer-movies.html',
  styleUrl: './customer-movies.css'
})
export class CustomerMovies implements OnInit {
  genres: any[] = [];
  selectedGenreIds = new Set<number>();
  selectedLanguages = new Set<string>();
  availableLanguages: string[] = [];
  movies: any[] = [];
  allMovies: any[] = [];
  pagedMovies: any[] = [];
  q = '';
  loading = true;
  currentPage = 1;
  pageSize = 30;
  private previousQ = '';
  rentalPriceByMovieId: Record<number, number> = {};
  minYear?: number;
  maxYear?: number;
  minPrice?: number;
  maxPrice?: number;
  /** Bounds for rental price slider (derived from inventory) */
  priceSliderMin = 0;
  priceSliderMax = 500;

  constructor(
    private genreService: GenreService,
    private movieService: MovieService,
    private inventoryService: InventoryService,
    private route: ActivatedRoute,
    private router: Router,
    private cdr: ChangeDetectorRef
  ) {}

  ngOnInit(): void {
    setTimeout(() => {
      if (this.loading) { this.loading = false; this.cdr.markForCheck(); }
    }, 5000);

    this.genreService.getAllGenres().subscribe((g) => (this.genres = g || []));
    this.loadInventoryPrices();

    this.route.queryParamMap.subscribe((params) => {
      this.q = (params.get('q') || '').trim();
      this.currentPage = Number(params.get('page') || 1);
      if (this.previousQ && !this.q) {
        this.selectedGenreIds.clear();
      }
      this.previousQ = this.q;
      this.fetchMovies();
    });
  }

  toggleGenre(genreId: number) {
    if (this.selectedGenreIds.has(genreId)) this.selectedGenreIds.delete(genreId);
    else this.selectedGenreIds.add(genreId);

    this.currentPage = 1;
    this.fetchMovies();
    this.router.navigate([], {
      relativeTo: this.route,
      queryParams: { page: 1 },
      queryParamsHandling: 'merge'
    });
  }

  toggleLanguage(language: string) {
    if (this.selectedLanguages.has(language)) this.selectedLanguages.delete(language);
    else this.selectedLanguages.add(language);
    this.applyClientFilters();
  }

  applyRangeFilters() {
    const min = Number(this.minPrice);
    const max = Number(this.maxPrice);
    if (Number.isFinite(min) && Number.isFinite(max) && min > max) {
      this.minPrice = max;
      this.maxPrice = min;
    }
    this.applyClientFilters();
  }

  clearFilters() {
    this.selectedGenreIds.clear();
    this.selectedLanguages.clear();
    this.minYear = undefined;
    this.maxYear = undefined;
    this.minPrice = undefined;
    this.maxPrice = undefined;
    this.fetchMovies();
  }

  openMovie(id: number) {
    this.router.navigate(['/dashboard/movie', id]);
  }

  get totalPages(): number {
    return Math.max(1, Math.ceil(this.movies.length / this.pageSize));
  }

  get pages(): number[] {
    return Array.from({ length: this.totalPages }, (_, i) => i + 1);
  }

  goPage(page: number) {
    this.currentPage = page;
    this.applyPagination();
    this.router.navigate([], {
      relativeTo: this.route,
      queryParams: { page },
      queryParamsHandling: 'merge'
    });
  }

  private applyPagination() {
    const total = this.totalPages;
    if (this.currentPage > total) {
      this.currentPage = 1;
    }
    const start = (this.currentPage - 1) * this.pageSize;
    this.pagedMovies = this.movies.slice(start, start + this.pageSize);
  }

  private refreshAvailableLanguages() {
    const langs = new Set<string>();
    (this.allMovies || []).forEach((m: any) => {
      const lang = String(m?.language || '').trim();
      if (lang) langs.add(lang);
    });
    this.availableLanguages = Array.from(langs).sort((a, b) => a.localeCompare(b));
  }

  private applyClientFilters() {
    const minYear = Number(this.minYear);
    const maxYear = Number(this.maxYear);
    const minPrice = Number(this.minPrice);
    const maxPrice = Number(this.maxPrice);

    this.movies = (this.allMovies || []).filter((m: any) => {
      const movieYear = Number(m?.releaseYear || 0);
      const lang = String(m?.language || '').trim();
      const price = Number(this.rentalPriceByMovieId[m.id]);

      if (this.selectedLanguages.size && !this.selectedLanguages.has(lang)) return false;
      if (Number.isFinite(minYear) && minYear > 0 && movieYear < minYear) return false;
      if (Number.isFinite(maxYear) && maxYear > 0 && movieYear > maxYear) return false;
      if (Number.isFinite(minPrice) && minPrice > 0 && (!Number.isFinite(price) || price < minPrice)) return false;
      if (Number.isFinite(maxPrice) && maxPrice > 0 && (!Number.isFinite(price) || price > maxPrice)) return false;
      return true;
    });

    this.currentPage = 1;
    this.applyPagination();
  }

  getRentalPrice(movieId: number): number | null {
    const price = this.rentalPriceByMovieId[movieId];
    return Number.isFinite(price) ? price : null;
  }

  private fetchMovies() {
    this.loading = true;
    if (this.q) {
      this.movieService.search(this.q, 1, 1000).pipe(
        timeout(10000),
        catchError(() => of([]))
      ).subscribe((res: any) => {
        const list = Array.isArray(res) ? res : (res?.items || []);
        this.allMovies = list;
        this.refreshAvailableLanguages();
        this.applyClientFilters();
        setTimeout(() => { this.loading = false; this.cdr.markForCheck(); }, 0);
      });
      return;
    }

    if (this.selectedGenreIds.size === 0) {
      this.movieService.getAll().pipe(
        timeout(10000),
        catchError(() => of([]))
      ).subscribe((all) => {
        this.allMovies = all || [];
        this.refreshAvailableLanguages();
        this.applyClientFilters();
        setTimeout(() => { this.loading = false; this.cdr.markForCheck(); }, 0);
      });
      return;
    }

    const calls = Array.from(this.selectedGenreIds).map((id) => {
      const genreName = this.genres.find((g) => g.id === id)?.name || '';
      if (!genreName) return of([]);
      return this.genreService.getMoviesByGenreName(genreName).pipe(catchError(() => of([])));
    });
    forkJoin(calls).pipe(
      timeout(10000),
      catchError(() => of([]))
    ).subscribe((arr: any) => {
      const map = new Map<number, any>();
      (arr as any[][]).flat().forEach((m: any) => map.set(m.id, m));
      this.allMovies = Array.from(map.values());
      this.refreshAvailableLanguages();
      this.applyClientFilters();
      setTimeout(() => { this.loading = false; this.cdr.markForCheck(); }, 0);
    });
  }

  private loadInventoryPrices() {
    this.inventoryService.getAll().pipe(
      timeout(10000),
      catchError(() => of([]))
    ).subscribe((rows: any[]) => {
      const map: Record<number, number> = {};
      (rows || []).forEach((r: any) => {
        const movieId = Number(r?.movieId);
        const price = Number(r?.rentalPrice);
        if (!movieId || !Number.isFinite(price)) return;
        if (!Number.isFinite(map[movieId]) || price < map[movieId]) {
          map[movieId] = price;
        }
      });
      this.rentalPriceByMovieId = map;
      const prices = Object.values(map).filter(Number.isFinite);
      if (prices.length) {
        this.priceSliderMin = Math.floor(Math.min(...prices));
        this.priceSliderMax = Math.ceil(Math.max(...prices));
        if (this.priceSliderMin === this.priceSliderMax) {
          this.priceSliderMax = this.priceSliderMin + 100;
        }
      }
      this.applyClientFilters();
    });
  }

  onMinPriceSliderChange(val: number) {
    this.minPrice = val;
    if (Number.isFinite(this.maxPrice) && (this.maxPrice ?? 0) < val) {
      this.maxPrice = val;
    }
    this.applyRangeFilters();
  }

  onMaxPriceSliderChange(val: number) {
    this.maxPrice = val;
    if (Number.isFinite(this.minPrice) && (this.minPrice ?? 0) > val) {
      this.minPrice = val;
    }
    this.applyRangeFilters();
  }

  getPriceMinPct(): number {
    const range = this.priceSliderMax - this.priceSliderMin || 1;
    const val = this.minPrice ?? this.priceSliderMin;
    return Math.min(100, Math.max(0, ((val - this.priceSliderMin) / range) * 100));
  }

  getPriceMaxPct(): number {
    const range = this.priceSliderMax - this.priceSliderMin || 1;
    const val = this.maxPrice ?? this.priceSliderMax;
    return Math.min(100, Math.max(0, ((val - this.priceSliderMin) / range) * 100));
  }
}

