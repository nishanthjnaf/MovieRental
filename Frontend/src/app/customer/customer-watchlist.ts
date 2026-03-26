import { ChangeDetectorRef, Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router } from '@angular/router';
import { WatchlistService } from '../services/watchlist';
import { CurrentUserService } from '../services/current-user';
import { MovieService } from '../services/movie';
import { ToastrService } from 'ngx-toastr';
import { CartStateService } from '../services/cart-state';

@Component({
  selector: 'app-customer-watchlist',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './customer-watchlist.html'
})
export class CustomerWatchlist implements OnInit {
  items: any[] = [];
  loading = true;
  showConfirmPopup = false;
  pendingRemoveId: number | null = null;

  constructor(
    private watchlistService: WatchlistService,
    private currentUser: CurrentUserService,
    private movieService: MovieService,
    private cart: CartStateService,
    private router: Router,
    private toastr: ToastrService,
    private cdr: ChangeDetectorRef
  ) {}

  ngOnInit(): void {
    this.load();
  }

  load() {
    const userId = this.currentUser.currentUserId || this.currentUser.decodedUserId;
    if (!userId) {
      this.loading = false;
      this.cdr.detectChanges();
      return;
    }
    this.watchlistService.getByUser(userId).subscribe({
      next: (list) => {
        const src = list || [];
        if (!src.length) {
          this.items = [];
          this.loading = false;
          this.cdr.detectChanges();
          return;
        }
        let pending = src.length;
        src.forEach((i) => {
          this.movieService.getById(i.movieId).subscribe({
            next: (movie) => {
              this.items = [...this.items.filter((x) => x.id !== i.id), { ...i, movie }];
              pending--;
              if (pending <= 0) {
                this.loading = false;
                this.cdr.detectChanges();
              }
            },
            error: () => {
              pending--;
              if (pending <= 0) {
                this.loading = false;
                this.cdr.detectChanges();
              }
            }
          });
        });
      },
      error: () => {
        this.items = [];
        this.loading = false;
        this.cdr.detectChanges();
      }
    });
  }

  open(movieId: number) {
    this.router.navigate(['/dashboard/movie', movieId]);
  }

  addToCart(item: any) {
    const movie = item?.movie;
    if (!movie?.id) return;
    const result = this.cart.add(movie);
    if (!result) return;
    result.subscribe({
      next: (res) => {
        if (res === null) {
          this.toastr.error('Could not add to cart');
        } else if (res === 'exists') {
          this.toastr.info('Already in cart');
        } else {
          this.watchlistService.remove(item.id).subscribe({
            next: () => {
              this.items = this.items.filter((i) => i.id !== item.id);
              this.toastr.success('Added to cart and removed from watchlist');
            },
            error: () => this.toastr.success('Added to cart')
          });
        }
      },
      error: () => this.toastr.error('Could not add to cart')
    });
  }

  remove(itemId: number) {
    this.pendingRemoveId = itemId;
    this.showConfirmPopup = true;
    this.cdr.detectChanges();
  }

  confirmRemove() {
    if (this.pendingRemoveId === null) return;
    const id = this.pendingRemoveId;
    // Optimistically update UI immediately
    this.items = this.items.filter((i) => i.id !== id);
    this.cancelRemove();
    this.watchlistService.remove(id).subscribe({
      next: () => this.toastr.success('Removed from watchlist'),
      error: () => this.toastr.error('Could not remove from watchlist')
    });
    this.cdr.detectChanges();
  }

  cancelRemove() {
    this.showConfirmPopup = false;
    this.pendingRemoveId = null;
    this.cdr.detectChanges();
  }
}

