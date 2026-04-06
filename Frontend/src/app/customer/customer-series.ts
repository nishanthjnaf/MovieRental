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
  templateUrl: './customer-series.html'
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
  hoveredSeriesId: number | null = null;
  hoveredSeasonId: number | null = null;

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
      this.availableLanguages = [...new Set(this.allSeries.map(s => s.language).filter(Boolean))];
      this.applyFilters();
      this.loading = false;
      this.cdr.detectChanges();
    });
  }

  applyFilters() {
    let result = [...this.allSeries];
    if (this.q.trim()) {
      const q = this.q.trim().toLowerCase();
      result = result.filter(s => s.title?.toLowerCase().includes(q) || s.director?.toLowerCase().includes(q));
    }
    if (this.selectedGenreIds.size > 0) {
      result = result.filter(s => (s.genres || []).some((g: string) =>
        this.genres.filter(gn => this.selectedGenreIds.has(gn.id)).map(gn => gn.name).includes(g)
      ));
    }
    if (this.selectedLanguages.size > 0) {
      result = result.filter(s => this.selectedLanguages.has(s.language));
    }
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

  open(id: number) {
    this.router.navigate(['/dashboard/series', id]);
  }

  getAverageRating(series: any): number {
    if (!series.seasons?.length) return 0;
    const ratings = series.seasons
      .filter((s: any) => s.averageRating > 0)
      .map((s: any) => s.averageRating);
    return ratings.length ? ratings.reduce((a: number, b: number) => a + b, 0) / ratings.length : 0;
  }

  setHoveredSeries(seriesId: number | null) {
    this.hoveredSeriesId = seriesId;
    if (!seriesId) this.hoveredSeasonId = null;
    this.cdr.detectChanges();
  }

  setHoveredSeason(seasonId: number | null) {
    this.hoveredSeasonId = seasonId;
    this.cdr.detectChanges();
  }

  getHoveredSeries(): any {
    return this.allSeries.find(s => s.id === this.hoveredSeriesId);
  }
}
