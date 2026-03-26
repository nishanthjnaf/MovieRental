import { Component, OnInit, OnDestroy, ChangeDetectorRef, ElementRef, ViewChild, AfterViewInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ActivatedRoute, Router } from '@angular/router';
import { FormsModule } from '@angular/forms';
import { MovieService } from '../services/movie';
import { GenreService } from '../services/genre';
import { InventoryService } from '../services/inventory';
import { catchError, of, timeout } from 'rxjs';

@Component({
  selector: 'app-customer-movies',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './customer-movies.html',
  styleUrl: './customer-movies.css'
})
export class CustomerMovies implements OnInit, AfterViewInit, OnDestroy {
  @ViewChild('sentinel') sentinelRef!: ElementRef;

  genres: any[] = [];
  selectedGenreIds = new Set<number>();
  selectedLanguages = new Set<string>();
  availableLanguages: string[] = [];

  allMovies: any[] = [];
  visibleMovies: any[] = [];

  q = '';
  loading = true;
  loadingMore = false;

  private pageSize = 20;
  private loadedCount = 0;
  private observer?: IntersectionObserver;
  private previousQ = '';

  rentalPriceByMovieId: Record<number, number> = {};
  minYear?: number;
  maxYear?: number;
  minPrice?: number;
  maxPrice?: number;
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
    this.genreService.getAllGenres().subscribe((g) => (this.genres = g || []));
    this.loadInventoryPrices();

    this.route.queryParamMap.subscribe((params) => {
      this.q = (params.get('q') || '').trim();
      if (this.previousQ && !this.q) this.selectedGenreIds.clear();
      this.previousQ = this.q;
      this.fetchMovies();
    });
  }

  ngAfterViewInit(): void {
    this.setupObserver();
  }

  ngOnDestroy(): void {
    this.observer?.disconnect();
  }

  private setupObserver() {
    this.observer?.disconnect();
    this.observer = new IntersectionObserver((entries) => {
      if (entries[0].isIntersecting && !this.loadingMore && !this.loading) {
        this.loadMore();
      }
    }, { threshold: 0.1 });
    if (this.sentinelRef?.nativeElement) {
      this.observer.observe(this.sentinelRef.nativeElement);
    }
  }

  private resetVisible() {
    this.loadedCount = 0;
    this.visibleMovies = [];
    this.loadMore();
  }

  private loadMore() {
    if (this.loadedCount >= this.allMovies.length) return;
    this.loadingMore = true;
    const next = this.allMovies.slice(this.loadedCount, this.loadedCount + this.pageSize);
    this.loadedCount += next.length;
    this.visibleMovies = [...this.visibleMovies, ...next];
    this.loadingMore = false;
    this.cdr.detectChanges();
  }

  get hasMore(): boolean {
    return this.loadedCount < this.allMovies.length;
  }

  toggleGenre(genreId: number) {
    if (this.selectedGenreIds.has(genreId)) this.selectedGenreIds.delete(genreId);
    else this.selectedGenreIds.add(genreId);
    this.fetchMovies();
  }

  toggleLanguage(language: string) {
    if (this.selectedLanguages.has(language)) this.selectedLanguages.delete(language);
    else this.selectedLanguages.add(language);
    this.fetchMovies();
  }

  applyRangeFilters() {
    const min = Number(this.minPrice);
    const max = Number(this.maxPrice);
    if (Number.isFinite(min) && Number.isFinite(max) && min > max) {
      this.minPrice = max;
      this.maxPrice = min;
    }
    this.fetchMovies();
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

  getRentalPrice(movieId: number): number | null {
    const price = this.rentalPriceByMovieId[movieId];
    return Number.isFinite(price) ? price : null;
  }

  private refreshAvailableLanguages(movies: any[]) {
    const langs = new Set<string>();
    movies.forEach((m: any) => {
      const lang = String(m?.language || '').trim();
      if (lang) langs.add(lang);
    });
    // Merge with existing so language chips don't disappear when filtering
    this.availableLanguages.forEach(l => langs.add(l));
    this.availableLanguages = Array.from(langs).sort((a, b) => a.localeCompare(b));
  }

  private fetchMovies() {
    this.loading = true;
    this.cdr.detectChanges();

    const params: any = {};
    if (this.q) params.searchTerm = this.q;
    if (this.selectedGenreIds.size) params.genreIds = Array.from(this.selectedGenreIds);
    if (this.selectedLanguages.size) params.languages = Array.from(this.selectedLanguages);
    if (this.minYear) params.minYear = this.minYear;
    if (this.maxYear) params.maxYear = this.maxYear;
    if (this.minPrice) params.minPrice = this.minPrice;
    if (this.maxPrice) params.maxPrice = this.maxPrice;

    this.movieService.filter(params).pipe(
      timeout(15000),
      catchError(() => of([]))
    ).subscribe((movies: any[]) => {
      const list = movies || [];
      // On unfiltered load, refresh language chips from full result
      if (!this.selectedLanguages.size && !this.q) {
        this.refreshAvailableLanguages(list);
      }
      this.allMovies = list;
      this.resetVisible();
      this.loading = false;
      this.cdr.detectChanges();
      setTimeout(() => this.setupObserver(), 100);
    });
  }

  private loadInventoryPrices() {
    this.inventoryService.getAll().pipe(
      timeout(10000), catchError(() => of([]))
    ).subscribe((rows: any[]) => {
      const map: Record<number, number> = {};
      (rows || []).forEach((r: any) => {
        const movieId = Number(r?.movieId);
        const price = Number(r?.rentalPrice);
        if (!movieId || !Number.isFinite(price)) return;
        if (!Number.isFinite(map[movieId]) || price < map[movieId]) map[movieId] = price;
      });
      this.rentalPriceByMovieId = map;
      const prices = Object.values(map).filter(Number.isFinite);
      if (prices.length) {
        this.priceSliderMin = Math.floor(Math.min(...prices));
        this.priceSliderMax = Math.ceil(Math.max(...prices));
        if (this.priceSliderMin === this.priceSliderMax) this.priceSliderMax = this.priceSliderMin + 100;
      }
      this.cdr.detectChanges();
    });
  }

  onMinPriceSliderChange(val: number) {
    this.minPrice = val;
    if (Number.isFinite(this.maxPrice) && (this.maxPrice ?? 0) < val) this.maxPrice = val;
    this.applyRangeFilters();
  }

  onMaxPriceSliderChange(val: number) {
    this.maxPrice = val;
    if (Number.isFinite(this.minPrice) && (this.minPrice ?? 0) > val) this.minPrice = val;
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
