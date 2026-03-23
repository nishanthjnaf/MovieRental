import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ActivatedRoute, Router } from '@angular/router';
import { MovieService } from '../services/movie';
import { switchMap } from 'rxjs';

@Component({
  selector: 'app-customer-search',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './customer-search.html'
})
export class CustomerSearch implements OnInit {
  movies: any[] = [];
  q = '';
  loading = true;
  pageNumber = 1;
  pageSize = 24;
  totalPages = 1;

  constructor(
    private route: ActivatedRoute,
    private router: Router,
    private movieService: MovieService
  ) {}

  ngOnInit(): void {
    this.route.queryParamMap
      .pipe(
        switchMap((params) => {
          this.q = (params.get('q') || '').trim();
          this.pageNumber = Number(params.get('page') || 1);
          this.loading = true;
          if (!this.q) {
            return this.movieService.getAll();
          }
          return this.movieService.search(this.q, this.pageNumber, this.pageSize);
        })
      )
      .subscribe({
        next: (res: any) => {
          this.movies = Array.isArray(res) ? res : (res?.items || []);
          this.totalPages = Number(res?.totalPages || 1);
          this.loading = false;
        },
        error: () => {
          this.movies = [];
          this.loading = false;
        }
      });
  }

  openMovie(id: number) {
    this.router.navigate(['/dashboard/movie', id]);
  }

  goToPage(page: number) {
    this.router.navigate([], {
      relativeTo: this.route,
      queryParams: { page },
      queryParamsHandling: 'merge'
    });
  }

  get pages(): number[] {
    return Array.from({ length: this.totalPages }, (_, i) => i + 1);
  }
}

