import { Component, OnInit, ChangeDetectionStrategy, ChangeDetectorRef } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router, RouterModule } from '@angular/router';
import { MovieService } from '../services/movie';
import { finalize } from 'rxjs/operators';
import { catchError, of, timeout } from 'rxjs';

@Component({
  selector: 'app-customer-home',
  standalone: true,
  imports: [CommonModule, RouterModule],
  templateUrl: './customer-home.html',
  changeDetection: ChangeDetectionStrategy.Default
})
export class CustomerHome implements OnInit {
  newMovies: any[] = [];
  topRatedMovies: any[] = [];
  topRentedMovies: any[] = [];
  loading = true;

  constructor(private movieService: MovieService, private router: Router, private cdr: ChangeDetectorRef) {}

  ngOnInit(): void {
    setTimeout(() => {
      if (this.loading) { this.loading = false; this.cdr.markForCheck(); }
    }, 5000);

    let pending = 3;
    const done = () => {
      pending--;
      if (pending <= 0) {
        setTimeout(() => { this.loading = false; this.cdr.markForCheck(); }, 0);
      }
    };

    this.movieService.getAll().pipe(
      timeout(10000),
      catchError(() => of([])),
      finalize(done)
    ).subscribe({
      next: (movies) => {
        const sortedByAdded = [...(movies || [])].sort((a, b) => (b.id || 0) - (a.id || 0));
        this.newMovies = sortedByAdded.slice(0, 10);
      },
      error: () => (this.newMovies = [])
    });

    this.movieService.getTopUserRated(10).pipe(
      timeout(10000),
      catchError(() => of([])),
      finalize(done)
    ).subscribe({
      next: (res) => (this.topRatedMovies = res || []),
      error: () => (this.topRatedMovies = [])
    });
    this.movieService.getTopRented(10).pipe(
      timeout(10000),
      catchError(() => of([])),
      finalize(done)
    ).subscribe({
      next: (res) => (this.topRentedMovies = res || []),
      error: () => (this.topRentedMovies = [])
    });
  }

  openMovie(movieId: number) {
    this.router.navigate(['/dashboard/movie', movieId]);
  }
}

