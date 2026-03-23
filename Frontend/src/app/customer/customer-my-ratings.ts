import { ChangeDetectorRef, Component, OnInit } from '@angular/core';
import { CommonModule, DatePipe } from '@angular/common';
import { CurrentUserService } from '../services/current-user';
import { ReviewService } from '../services/review';
import { ToastrService } from 'ngx-toastr';
import { Router } from '@angular/router';

@Component({
  selector: 'app-customer-my-ratings',
  standalone: true,
  imports: [CommonModule, DatePipe],
  templateUrl: './customer-my-ratings.html'
})
export class CustomerMyRatings implements OnInit {
  ratings: any[] = [];
  loading = true;

  constructor(
    private currentUser: CurrentUserService,
    private reviewService: ReviewService,
    private toastr: ToastrService,
    private router: Router,
    private cdr: ChangeDetectorRef
  ) {}

  ngOnInit(): void {
    const userId = this.currentUser.currentUserId || this.currentUser.decodedUserId;
    if (!userId) {
      this.loading = false;
      this.cdr.detectChanges();
      return;
    }
    this.reviewService.getByUser(userId).subscribe({
      next: (res) => {
        this.ratings = res || [];
        this.loading = false;
        this.cdr.detectChanges();
      },
      error: () => {
        this.ratings = [];
        this.loading = false;
        this.cdr.detectChanges();
      }
    });
  }

  removeRating(id: number) {
    this.reviewService.deleteReview(id).subscribe({
      next: () => {
        this.ratings = this.ratings.filter((r) => r.id !== id);
        this.toastr.success('Rating removed');
        this.cdr.detectChanges();
      },
      error: () => this.toastr.error('Could not remove rating')
    });
  }

  openMovie(movieId: number) {
    this.router.navigate(['/dashboard/movie', movieId]);
  }
}

