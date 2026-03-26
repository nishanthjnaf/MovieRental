import { Component, OnInit, ChangeDetectorRef } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ActivatedRoute, Router } from '@angular/router';
import { DomSanitizer, SafeResourceUrl } from '@angular/platform-browser';
import { MovieService } from '../services/movie';
import { CurrentUserService } from '../services/current-user';
import { NotificationService } from '../services/notification';
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
  private movieId = 0;

  constructor(
    private route: ActivatedRoute,
    private router: Router,
    private movieService: MovieService,
    private currentUser: CurrentUserService,
    private notifService: NotificationService,
    private sanitizer: DomSanitizer,
    private cdr: ChangeDetectorRef
  ) {}

  ngOnInit(): void {
    this.movieId = Number(this.route.snapshot.paramMap.get('id'));
    if (!this.movieId) { this.router.navigate(['/dashboard/rentals']); return; }

    this.movieService.getById(this.movieId).pipe(catchError(() => of(null))).subscribe(m => {
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

      // Push rate-movie notification after 30 seconds of watching
      setTimeout(() => {
        const userId = this.currentUser.currentUserId || this.currentUser.decodedUserId;
        if (userId && this.movieId) {
          this.notifService.pushRateMovie(userId, this.movieId, this.movieTitle).subscribe();
        }
      }, 30000);
    });
  }

  goBack() {
    this.router.navigate(['/dashboard/rentals']);
  }
}
