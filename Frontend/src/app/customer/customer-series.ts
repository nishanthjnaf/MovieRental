import { Component, OnInit, ChangeDetectorRef } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router } from '@angular/router';
import { FormsModule } from '@angular/forms';
import { SeriesService } from '../services/series';
import { GenreService } from '../services/genre';
import { catchError, of } from 'rxjs';

@Component({
  selector: 'app-customer-series',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './customer-series.html',
  styleUrl: './customer-movies.css'
})
export class CustomerSeries implements OnInit {
  allSeries: any[] = [];
  visibleSeries: any[] = [];
  genres: any[] = [];
  selectedGenreIds = new Set<number>();
  selectedLanguages = new Set<string>();
  availableLanguages: string[] = [];

  q = '';
  loading = true;
  sortOption = '';
  minPrice?: number;
  maxPrice?: number;
  minSeasons?: number;
  maxSeasons?: number;
  priceSliderMin = 0;
  priceSliderMax = 500;

  constructor(
    private seriesService: SeriesService,
    private genreService: GenreService,
    private router: Router,
    private cdr: ChangeDetectorRef
  ) {}

  ngOnInit(): void {
    this.genreService.getAllGenres().subscribe(g => this.genres = g || []);
    this.load();
  }

  load() {
    this.loading = true;
    this.seriesService.getAll().pipe(catchError(() => of([]))).subscribe(list => {
      this.allSeries = list || [];
      this.availableLanguages = [...new Set(this.allSeries.map(s => s.language).filter(Boolean))].sort();
      const prices = this.allSeries.map(s => s.rentalPrice).filter(p => p > 0);
      if (prices.length) {
        this.priceSliderMin = Math.floor(Math.min(...prices));
        this.priceSliderMax = Math.ceil(Math.max(...prices));
      }
      this.applyFilters();
      this.loading = false;
      this.cdr.detectChanges();
    });
  }

  applyFilters() {
    let result = [...this.allSeries];

    if (this.q.trim()) {
      const q = this.q.trim().toLowerCase();
      result = result.filter(s =>
        s.title?.toLowerCase().includes(q) ||
        s.director?.toLowerCase().includes(q) ||
        (Array.isArray(s.cast) ? s.cast.join(' ') : s.cast || '').toLowerCase().includes(q)
      );
    }

    if (this.selectedGenreIds.size > 0) {
      const selectedNames = this.genres.filter(g => this.selectedGenreIds.has(g.id)).map(g => g.name);
      result = result.filter(s => (s.genres || []).some((g: string) => selectedNames.includes(g)));
    }

    if (this.selectedLanguages.size > 0) {
      result = result.filter(s => this.selectedLanguages.has(s.language));
    }

    if (this.minPrice != null) result = result.filter(s => s.rentalPrice >= this.minPrice!);
    if (this.maxPrice != null) result = result.filter(s => s.rentalPrice <= this.maxPrice!);
    if (this.minSeasons != null) result = result.filter(s => (s.seasons?.length ?? 0) >= this.minSeasons!);
    if (this.maxSeasons != null) result = result.filter(s => (s.seasons?.length ?? 0) <= this.maxSeasons!);

    // Sort
    if (this.sortOption === 'price-asc') result.sort((a, b) => a.rentalPrice - b.rentalPrice);
    else if (this.sortOption === 'price-desc') result.sort((a, b) => b.rentalPrice - a.rentalPrice);
    else if (this.sortOption === 'rating-desc') result.sort((a, b) => this.getAverageRating(b) - this.getAverageRating(a));
    else if (this.sortOption === 'rating-asc') result.sort((a, b) => this.getAverageRating(a) - this.getAverageRating(b));
    else if (this.sortOption === 'seasons-desc') result.sort((a, b) => (b.seasons?.length ?? 0) - (a.seasons?.length ?? 0));
    else if (this.sortOption === 'seasons-asc') result.sort((a, b) => (a.seasons?.length ?? 0) - (b.seasons?.length ?? 0));
    else if (this.sortOption === 'rentals-desc') result.sort((a, b) => (b.rentalCount ?? 0) - (a.rentalCount ?? 0));

    this.visibleSeries = result;
    this.cdr.detectChanges();
  }

  toggleGenre(id: number) {
    if (this.selectedGenreIds.has(id)) this.selectedGenreIds.delete(id);
    else this.selectedGenreIds.add(id);
    this.applyFilters();
  }

  toggleLanguage(lang: string) {
    if (this.selectedLanguages.has(lang)) this.selectedLanguages.delete(lang);
    else this.selectedLanguages.add(lang);
    this.applyFilters();
  }

  clearFilters() {
    this.selectedGenreIds.clear();
    this.selectedLanguages.clear();
    this.q = '';
    this.sortOption = '';
    this.minPrice = undefined;
    this.maxPrice = undefined;
    this.minSeasons = undefined;
    this.maxSeasons = undefined;
    this.applyFilters();
  }

  get activeFilterCount(): number {
    return this.selectedGenreIds.size + this.selectedLanguages.size +
      (this.minPrice != null ? 1 : 0) + (this.maxPrice != null ? 1 : 0) +
      (this.minSeasons != null ? 1 : 0) + (this.maxSeasons != null ? 1 : 0) +
      (this.sortOption ? 1 : 0);
  }

  getPriceMinPct(): number {
    const range = this.priceSliderMax - this.priceSliderMin || 1;
    return Math.min(100, Math.max(0, (((this.minPrice ?? this.priceSliderMin) - this.priceSliderMin) / range) * 100));
  }

  getPriceMaxPct(): number {
    const range = this.priceSliderMax - this.priceSliderMin || 1;
    return Math.min(100, Math.max(0, (((this.maxPrice ?? this.priceSliderMax) - this.priceSliderMin) / range) * 100));
  }

  open(id: number) { this.router.navigate(['/dashboard/series', id]); }

  getAverageRating(series: any): number {
    if (!series.seasons?.length) return 0;
    const ratings = series.seasons.filter((s: any) => s.averageRating > 0).map((s: any) => s.averageRating);
    return ratings.length ? ratings.reduce((a: number, b: number) => a + b, 0) / ratings.length : 0;
  }

  hasNewSeason(series: any): boolean {
    return (series.seasons || []).some((s: any) => s.isNewSeason);
  }
}
