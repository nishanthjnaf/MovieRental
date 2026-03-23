import { ChangeDetectorRef, Component, OnInit } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { CommonModule } from '@angular/common';
import { GenreService } from '../services/genre';
import { ToastrService } from 'ngx-toastr';
import { InventoryService } from '../services/inventory';
import { MovieService } from '../services/movie';
import { RentalService } from '../services/rental';
import { PaymentService } from '../services/payment';
import { Router } from '@angular/router';
import { CurrentUserService } from '../services/current-user';
import { UserService } from '../services/user';
import { catchError, forkJoin, of } from 'rxjs';

@Component({
  selector: 'app-admin',
  standalone: true,
  templateUrl: './admin.html',
  imports: [CommonModule, FormsModule]
})
export class Admin implements OnInit {

  constructor(
    private genreService: GenreService,
    private inventoryService: InventoryService,
    private movieService: MovieService,
    private rentalService: RentalService,
    private paymentService: PaymentService,
    private userService: UserService,
    private currentUser: CurrentUserService,
    private router: Router,
    private toastr: ToastrService,
    private cdr: ChangeDetectorRef
  ) {}

  viewMode: 'services' | 'genre' | 'inventory' | 'movie' | 'rental' | 'payment' = 'services';

  // ---- GENRE ----
  genres: any[] = [];
  searchId: any;
  showForm = false;
  isEdit = false;
  selectedId: number | null = null;
  formData = { name: '', description: '' };
  showAssign = false;
  selectedGenreId: number | null = null;
  assignMovieId: number | null = null;

  // ---- INVENTORY ----
  inventories: any[] = [];
  inventorySearchId: any;
  showInventoryForm = false;
  isInventoryEdit = false;
  selectedInventoryId: number | null = null;
  inventoryForm = { movieId: 0, rentalPrice: 0, isAvailable: true };

  // ---- MOVIE ----
  movies: any[] = [];
  movieSearchId: any;
  movieSearchName: string = '';
  showMovieForm = false;
  isMovieEdit = false;
  selectedMovieId: number | null = null;
  movieForm = {
    title: '', description: '', releaseYear: 2024, durationMinutes: 0,
    language: '', director: '', cast: '', contentRating: '',
    contentAdvisory: '', posterPath: '', trailerUrl: ''
  };
  showPoster = '';
  showTrailer = '';

  // ---- RENTAL ----
  rentals: any[] = [];
  rentalSearchUserId: any;
  showRentalItems = false;
  selectedRentalId: number | null = null;
  rentalItems: any[] = [];

  // ---- PAYMENT ----
  payments: any[] = [];
  paymentSearchRentalId: any;
  paymentSearchUserId: any;

  // ---- MISC ----
  currentAdminName = 'Admin';
  showProfileMenu = false;
  stats = { activeUsers: 0, activeRentalItems: 0, topRentedMovie: '-', revenueCollected: 0 };

  ngOnInit() {
    this.currentUser.loadCurrentUser().subscribe((u) => {
      this.currentAdminName = u?.name || u?.username || 'Admin';
      this.cdr.detectChanges();
    });
    this.loadDashboardStats();
  }

  openService(service: string) {
    this.viewMode = service as any;
    this.cdr.detectChanges();
    if (service === 'genre') this.loadGenres();
    if (service === 'inventory') this.loadInventory();
    if (service === 'movie') this.loadMovies();
    if (service === 'rental') this.loadRentals();
    if (service === 'payment') this.loadPayments();
  }

  // ================= STATS =================
  loadDashboardStats() {
    forkJoin({
      users: this.userService.getAll().pipe(catchError(() => of([]))),
      rentals: this.rentalService.getAll().pipe(catchError(() => of([]))),
      payments: this.paymentService.getAll().pipe(catchError(() => of([]))),
      topRented: this.movieService.getTopRented(1).pipe(catchError(() => of([])))
    }).subscribe({
      next: ({ users, rentals, payments, topRented }) => {
        const allUsers = this.extractList(users);
        const allRentals = this.extractList(rentals);
        const allPayments = this.extractList(payments);
        const topList = this.extractList(topRented);

        this.stats.activeUsers = allUsers.filter((u: any) => (u?.role || '').toLowerCase() === 'customer').length;
        this.stats.topRentedMovie = topList[0]?.title || '-';
        this.stats.revenueCollected = allPayments
          .filter((p: any) => Number(p?.status ?? p?.paymentStatus) === 0)
          .reduce((sum: number, p: any) => sum + Number(p?.amount || 0), 0);

        const rentalIds = allRentals.map((r: any) => r?.id).filter((id: any) => Number(id) > 0);
        if (!rentalIds.length) {
          this.stats.activeRentalItems = 0;
          this.cdr.detectChanges();
          return;
        }

        forkJoin(
          rentalIds.map((id: number) =>
            this.rentalService.getItemsByRentalId(id).pipe(catchError(() => of([])))
          )
        ).subscribe((itemGroups: any[]) => {
          this.stats.activeRentalItems = itemGroups.flat().filter((item: any) => !!item?.isActive).length;
          this.cdr.detectChanges();
        });

        this.cdr.detectChanges();
      }
    });
  }

  private extractList(res: any): any[] {
    if (Array.isArray(res)) return res;
    if (Array.isArray(res?.data)) return res.data;
    if (Array.isArray(res?.items)) return res.items;
    if (res?.data) return [res.data];
    return [];
  }

  toggleProfileMenu() { this.showProfileMenu = !this.showProfileMenu; this.cdr.detectChanges(); }
  openProfile() { this.showProfileMenu = false; this.router.navigate(['/admin/profile']); }

  // ================= GENRE =================
  loadGenres() {
    this.genreService.getAllGenres().subscribe({
      next: (res) => { this.genres = [...res]; this.cdr.detectChanges(); },
      error: () => this.toastr.error('Failed to load genres')
    });
  }

  refreshGenres() { this.loadGenres(); this.toastr.success('Refreshed'); }

  searchGenre() {
    if (!this.searchId) { this.loadGenres(); return; }
    this.genreService.getGenreById(this.searchId).subscribe({
      next: (res) => { this.genres = [res]; this.cdr.detectChanges(); },
      error: () => this.toastr.error('Genre not found')
    });
  }

  resetSearch() { this.searchId = null; this.loadGenres(); }

  openAddForm() {
    this.isEdit = false;
    this.formData = { name: '', description: '' };
    this.showForm = true;
    this.cdr.detectChanges();
  }

  editGenre(g: any) {
    this.isEdit = true;
    this.selectedId = g.id;
    this.formData = { name: g.name, description: g.description };
    this.showForm = true;
    this.cdr.detectChanges();
  }

  saveGenre() {
    if (this.isEdit && this.selectedId !== null) {
      this.genreService.updateGenre(this.selectedId, this.formData).subscribe({
        next: () => { this.toastr.success('Updated'); this.cancelForm(); this.loadGenres(); },
        error: () => this.toastr.error('Update failed')
      });
    } else {
      this.genreService.addGenre(this.formData).subscribe({
        next: () => { this.toastr.success('Added'); this.cancelForm(); this.loadGenres(); },
        error: () => this.toastr.error('Add failed')
      });
    }
  }

  cancelForm() { this.showForm = false; this.cdr.detectChanges(); }

  deleteGenre(id: number) {
    if (!confirm('Delete this genre?')) return;
    this.genreService.deleteGenre(id).subscribe({
      next: () => { this.toastr.success('Deleted'); this.loadGenres(); },
      error: () => { this.toastr.success('Deleted'); this.loadGenres(); }
    });
  }

  openAssign(g: any) {
    this.selectedGenreId = g.id;
    this.assignMovieId = null;
    this.showAssign = true;
    this.cdr.detectChanges();
  }

  assignMovie() {
    if (!this.selectedGenreId || !this.assignMovieId) { this.toastr.error('Enter valid Movie ID'); return; }
    this.genreService.assignMovie(this.selectedGenreId, this.assignMovieId).subscribe({
      next: () => { this.toastr.success('Movie assigned'); this.cancelAssign(); this.loadGenres(); },
      error: () => this.toastr.error('Assignment failed')
    });
  }

  cancelAssign() { this.showAssign = false; this.cdr.detectChanges(); }

  // ================= INVENTORY =================
  loadInventory() {
    this.inventoryService.getAll().subscribe({
      next: (res) => { this.inventories = [...res]; this.cdr.detectChanges(); },
      error: () => this.toastr.error('Failed to load inventory')
    });
  }

  refreshInventory() { this.loadInventory(); this.toastr.success('Refreshed'); }

  searchInventory() {
    if (!this.inventorySearchId) { this.loadInventory(); return; }
    this.inventoryService.getById(this.inventorySearchId).subscribe({
      next: (res) => { this.inventories = [res]; this.cdr.detectChanges(); },
      error: () => this.toastr.error('Inventory not found')
    });
  }

  resetInventory() { this.inventorySearchId = null; this.loadInventory(); }

  openInventoryForm() {
    this.isInventoryEdit = false;
    this.inventoryForm = { movieId: 0, rentalPrice: 0, isAvailable: true };
    this.showInventoryForm = true;
    this.cdr.detectChanges();
  }

  editInventory(i: any) {
    this.isInventoryEdit = true;
    this.selectedInventoryId = i.id;
    this.inventoryForm = { movieId: i.movieId, rentalPrice: i.rentalPrice, isAvailable: i.isAvailable };
    this.showInventoryForm = true;
    this.cdr.detectChanges();
  }

  saveInventory() {
    if (this.isInventoryEdit && this.selectedInventoryId !== null) {
      this.inventoryService.update(this.selectedInventoryId, this.inventoryForm).subscribe({
        next: () => { this.toastr.success('Updated'); this.showInventoryForm = false; this.loadInventory(); },
        error: () => this.toastr.error('Update failed')
      });
    } else {
      this.inventoryService.add(this.inventoryForm).subscribe({
        next: () => { this.toastr.success('Added'); this.showInventoryForm = false; this.loadInventory(); },
        error: () => this.toastr.error('Add failed')
      });
    }
  }

  cancelInventoryForm() { this.showInventoryForm = false; this.cdr.detectChanges(); }

  deleteInventory(id: number) {
    if (!confirm('Delete this inventory?')) return;
    this.inventoryService.delete(id).subscribe({
      next: () => { this.toastr.success('Deleted'); this.loadInventory(); },
      error: () => { this.toastr.success('Deleted'); this.loadInventory(); }
    });
  }

  toggleAvailability(id: number) {
    if (!confirm('Change availability status?')) return;
    this.inventoryService.toggle(id).subscribe({
      next: () => { this.toastr.success('Status updated'); this.loadInventory(); },
      error: () => { this.toastr.success('Status updated'); this.loadInventory(); }
    });
  }

  // ================= MOVIE =================
  loadMovies() {
    this.movieService.getAll().subscribe({
      next: (res) => { this.movies = [...res]; this.cdr.detectChanges(); },
      error: () => this.toastr.error('Failed to load movies')
    });
  }

  refreshMovies() { this.loadMovies(); this.toastr.success('Refreshed'); }

  searchMovie() {
    if (this.movieSearchId) {
      this.movieService.getById(this.movieSearchId).subscribe({
        next: (res) => { this.movies = [res]; this.cdr.detectChanges(); },
        error: () => this.toastr.error('Movie not found')
      });
      return;
    }
    if (this.movieSearchName) {
      this.movieService.search(this.movieSearchName).subscribe({
        next: (res: any) => {
          this.movies = Array.isArray(res) ? res : (res?.items || res?.data || res?.results || []);
          this.cdr.detectChanges();
        },
        error: () => this.toastr.error('Search failed')
      });
      return;
    }
    this.loadMovies();
  }

  resetMovie() { this.movieSearchId = null; this.movieSearchName = ''; this.loadMovies(); }

  openMovieForm() {
    this.isMovieEdit = false;
    this.movieForm = {
      title: '', description: '', releaseYear: 2024, durationMinutes: 0,
      language: '', director: '', cast: '', contentRating: '',
      contentAdvisory: '', posterPath: '', trailerUrl: ''
    };
    this.showMovieForm = true;
    this.cdr.detectChanges();
  }

  editMovie(m: any) {
    this.isMovieEdit = true;
    this.selectedMovieId = m.id;
    this.movieForm = {
      title: m.title ?? '',
      description: m.description ?? '',
      releaseYear: m.releaseYear ?? 2024,
      durationMinutes: m.durationMinutes ?? 0,
      language: m.language ?? '',
      director: m.director ?? '',
      cast: Array.isArray(m.cast) ? m.cast.join(', ') : (m.cast ?? ''),
      contentRating: m.contentRating ?? '',
      contentAdvisory: Array.isArray(m.contentAdvisory) ? m.contentAdvisory.join(', ') : (m.contentAdvisory ?? ''),
      posterPath: m.posterPath ?? '',
      trailerUrl: m.trailerUrl ?? ''
    };
    this.showMovieForm = true;
    this.cdr.detectChanges();
  }

  saveMovie() {
    if (this.isMovieEdit && this.selectedMovieId !== null) {
      this.movieService.update(this.selectedMovieId, this.movieForm).subscribe({
        next: () => { this.toastr.success('Updated'); this.showMovieForm = false; this.loadMovies(); },
        error: () => this.toastr.error('Update failed')
      });
    } else {
      this.movieService.add(this.movieForm).subscribe({
        next: () => { this.toastr.success('Added'); this.showMovieForm = false; this.loadMovies(); },
        error: () => this.toastr.error('Add failed')
      });
    }
  }

  cancelMovieForm() { this.showMovieForm = false; this.cdr.detectChanges(); }

  deleteMovie(id: number) {
    if (!confirm('Delete this movie?')) return;
    this.movieService.delete(id).subscribe({
      next: () => { this.toastr.success('Deleted'); this.loadMovies(); },
      error: () => { this.toastr.success('Deleted'); this.loadMovies(); }
    });
  }

  showPosterPopup(url: string) { this.showPoster = url; this.cdr.detectChanges(); }
  showTrailerPopup(url: string) { this.showTrailer = url; this.cdr.detectChanges(); }

  // ================= RENTAL =================
  loadRentals() {
    this.rentalService.getAll().subscribe({
      next: (res: any) => {
        this.rentals = Array.isArray(res) ? res : (res?.data || res?.items || []);
        this.cdr.detectChanges();
      },
      error: () => this.toastr.error('Failed to load rentals')
    });
  }

  refreshRentals() { this.loadRentals(); this.toastr.success('Refreshed'); }

  searchRental() {
    if (!this.rentalSearchUserId) { this.loadRentals(); return; }
    this.rentalService.getByUserId(this.rentalSearchUserId).subscribe({
      next: (res: any) => {
        this.rentals = Array.isArray(res) ? res : (res?.data || []);
        this.cdr.detectChanges();
      },
      error: () => this.toastr.error('No rentals found')
    });
  }

  resetRental() { this.rentalSearchUserId = null; this.loadRentals(); }

  getStatusText(status: number): string {
    switch (status) {
      case 0: return 'Payment Pending';
      case 1: return 'Available';
      case 2: return 'Payment Declined';
      case 3: return 'Payment Not Done';
      default: return 'Unknown';
    }
  }

  openRentalItems(rentalId: number) {
    this.selectedRentalId = rentalId;
    this.rentalService.getItemsByRentalId(rentalId).subscribe({
      next: (res: any) => {
        this.rentalItems = Array.isArray(res) ? res : (res?.data || []);
        this.showRentalItems = true;
        this.cdr.detectChanges();
      },
      error: () => this.toastr.error('Failed to load items')
    });
  }

  closeRentalItems() { this.showRentalItems = false; this.cdr.detectChanges(); }

  endRentalItem(item: any) {
    if (!item?.id) { this.toastr.error('Invalid Rental Item ID'); return; }
    if (!confirm('End this rental item?')) return;
    this.rentalService.endItem(item.id).subscribe({
      next: () => { this.toastr.success('Rental ended'); this.openRentalItems(this.selectedRentalId!); },
      error: () => { this.toastr.success('Rental ended'); this.openRentalItems(this.selectedRentalId!); }
    });
  }

  // ================= PAYMENT =================
  loadPayments() {
    this.paymentService.getAll().subscribe({
      next: (res: any) => {
        this.payments = Array.isArray(res) ? res : (res?.data || []);
        this.cdr.detectChanges();
      },
      error: () => this.toastr.error('Failed to load payments')
    });
  }

  refreshPayments() { this.loadPayments(); this.toastr.success('Refreshed'); }

  searchPayment() {
    if (this.paymentSearchRentalId) {
      this.paymentService.getByRentalId(this.paymentSearchRentalId).subscribe({
        next: (res: any) => {
          this.payments = Array.isArray(res) ? res : (res ? [res] : []);
          this.cdr.detectChanges();
        },
        error: () => this.toastr.error('No payments found')
      });
      return;
    }
    if (this.paymentSearchUserId) {
      this.paymentService.getByUserId(this.paymentSearchUserId).subscribe({
        next: (res: any) => {
          this.payments = Array.isArray(res) ? res : (res?.data || []);
          this.cdr.detectChanges();
        },
        error: () => this.toastr.error('No payments found')
      });
      return;
    }
    this.loadPayments();
  }

  resetPayment() { this.paymentSearchRentalId = null; this.paymentSearchUserId = null; this.loadPayments(); }

  getPaymentStatus(status: number): string {
    return status === 0 ? 'Success' : 'Failed';
  }

  getPaymentMethod(method: number): string {
    switch (method) {
      case 0: return 'Debit Card';
      case 1: return 'Credit Card';
      case 2: return 'Net Banking';
      case 3: return 'UPI';
      default: return '-';
    }
  }

  // ================= AUTH =================
  logout() {
    this.showProfileMenu = false;
    localStorage.removeItem('token');
    sessionStorage.removeItem('token');
    localStorage.removeItem('role');
    this.currentUser.clear();
    this.toastr.info('Logged out');
    this.router.navigate(['/login']);
  }
}
