import { Component, OnInit, OnDestroy, ViewChild, ElementRef, ChangeDetectorRef } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ActivatedRoute, Router } from '@angular/router';
import { DomSanitizer, SafeResourceUrl } from '@angular/platform-browser';
import { MovieService } from '../services/movie';
import { catchError, of } from 'rxjs';

@Component({
  selector: 'app-customer-watch',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './customer-watch.html'
})
export class CustomerWatch implements OnInit {
  movieTitle = '';
  embedUrl: SafeResourceUrl | null = null;
  loading = true;

  constructor(
    private route: ActivatedRoute,
    private router: Router,
    private movieService: MovieService,
    private sanitizer: DomSanitizer,
    private cdr: ChangeDetectorRef
  ) {}

  ngOnInit(): void {
    const movieId = Number(this.route.snapshot.paramMap.get('id'));
    if (!movieId) { this.router.navigate(['/dashboard/rentals']); return; }

    this.movieService.getById(movieId).pipe(catchError(() => of(null))).subscribe(m => {
      this.movieTitle = m?.title || 'Movie';
      const url = m?.trailerUrl;
      let embedUrl = 'https://www.youtube.com/embed/dQw4w9WgXcQ?autoplay=1&controls=1&cc_load_policy=1';
      if (url) {
        const match = url.match(/(?:youtu\.be\/|youtube\.com\/(?:watch\?v=|embed\/|v\/))([A-Za-z0-9_-]{11})/);
        if (match?.[1]) embedUrl = `https://www.youtube.com/embed/${match[1]}?autoplay=1&controls=1&cc_load_policy=1`;
      }
      this.embedUrl = this.sanitizer.bypassSecurityTrustResourceUrl(embedUrl);
      this.loading = false;
      this.cdr.detectChanges();
    });
  }

  goBack() {
    this.router.navigate(['/dashboard/rentals']);
  }
}
