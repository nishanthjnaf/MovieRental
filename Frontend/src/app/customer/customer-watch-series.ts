import { Component, OnInit, ChangeDetectorRef } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ActivatedRoute, Router } from '@angular/router';
import { DomSanitizer, SafeResourceUrl } from '@angular/platform-browser';
import { SeriesService } from '../services/series';
import { catchError, of } from 'rxjs';

@Component({
  selector: 'app-customer-watch-series',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './customer-watch-series.html'
})
export class CustomerWatchSeries implements OnInit {
  series: any = null;
  loading = true;
  selectedSeason: any = null;
  selectedEpisode: any = null;
  embedUrl: SafeResourceUrl | null = null;
  private seriesId = 0;

  constructor(
    private route: ActivatedRoute,
    private router: Router,
    private seriesService: SeriesService,
    private sanitizer: DomSanitizer,
    private cdr: ChangeDetectorRef
  ) {}

  ngOnInit(): void {
    this.seriesId = Number(this.route.snapshot.paramMap.get('id'));
    if (!this.seriesId) { this.router.navigate(['/dashboard/rentals']); return; }

    this.seriesService.getById(this.seriesId).pipe(catchError(() => of(null))).subscribe(s => {
      this.series = s;
      if (s?.seasons?.length) {
        this.selectSeason(s.seasons[0]);
      }
      this.loading = false;
      this.cdr.detectChanges();
    });
  }

  selectSeason(season: any) {
    this.selectedSeason = season;
    if (season?.episodes?.length) {
      this.selectEpisode(season.episodes[0]);
    } else {
      this.selectedEpisode = null;
      this.embedUrl = null;
    }
    this.cdr.detectChanges();
  }

  selectEpisode(ep: any) {
    this.selectedEpisode = ep;
    // Use the series trailer URL as the video source (placeholder for actual episode video)
    const url = this.series?.trailerUrl;
    let embed = 'https://www.youtube.com/embed/dQw4w9WgXcQ?autoplay=1&controls=1';
    if (url) {
      const match = url.match(/(?:youtu\.be\/|youtube\.com\/(?:watch\?v=|embed\/|v\/))([A-Za-z0-9_-]{11})/);
      if (match?.[1]) embed = `https://www.youtube.com/embed/${match[1]}?autoplay=1&controls=1`;
    }
    this.embedUrl = this.sanitizer.bypassSecurityTrustResourceUrl(embed);
    this.cdr.detectChanges();
  }

  goBack() {
    this.router.navigate(['/dashboard/series', this.seriesId]);
  }
}
