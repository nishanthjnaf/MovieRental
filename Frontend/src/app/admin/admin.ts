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
import { NotificationService } from '../services/notification';
import { catchError, forkJoin, of } from 'rxjs';
import { ThemeToggle } from '../components/theme-toggle';

@Component({
  selector: 'app-admin',
  standalone: true,
  templateUrl: './admin.html',
  imports: [CommonModule, FormsModule, ThemeToggle]
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
    private notifService: NotificationService,
    private router: Router,
    private toastr: ToastrService,
    private cdr: ChangeDetectorRef
  ) {}

  viewMode: 'services' | 'genre' | 'inventory' | 'movie' | 'rental' | 'payment' | 'broadcast' = 'services';

  // ---- BROADCAST ----
  broadcastTitle = '';
  broadcastMessage = '';
  broadcasts: any[] = [];
  filteredBroadcasts: any[] = [];
  broadcastsLoading = false;
  broadcastFilterUsername = '';
  broadcastDateFrom = '';
  broadcastDateTo = '';
  broadcastSortDate = '';

  // ---- GENRE ----
  genres: any[] = [];
  searchId: any;
  genreSearchName = '';
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
  inventorySearchMovieId: any;
  inventorySearchMovieName = '';
  inventoryStatusFilter = '';
  showInventoryForm = false;
  isInventoryEdit = false;
  selectedInventoryId: number | null = null;
  inventoryForm = { movieId: 0, rentalPrice: 0, isAvailable: true };

  // ---- MOVIE ----
  movies: any[] = [];
  movieLanguages: string[] = [];
  movieSearchId: any;
  movieSearchName = '';
  movieSearchLanguage = '';
  movieMinYear: any;
  movieMaxYear: any;
  movieSort = '';
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
  rentalDateFrom = '';
  rentalDateTo = '';
  rentalStatusFilter = '';
  rentalSortDate = '';
  showRentalItems = false;

  get todayStr(): string {
    return new Date().toISOString().split('T')[0];
  }
  selectedRentalId: number | null = null;
  rentalItems: any[] = [];

  // ---- PAYMENT ----
  payments: any[] = [];
  paymentSearchRentalId: any;
  paymentSearchUserId: any;
  paymentMethodFilter = '';
  paymentStatusFilter = '';
  paymentDateFrom = '';
  paymentDateTo = '';
  paymentSortDate = '';

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
    if (service === 'broadcast') this.loadBroadcasts();
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
    if (this.searchId) {
      this.genreService.getGenreById(this.searchId).subscribe({
        next: (res) => { this.genres = [res]; this.cdr.detectChanges(); },
        error: () => this.toastr.error('Genre not found')
      });
      return;
    }
    if (this.genreSearchName.trim()) {
      this.genreService.getAllGenres().subscribe({
        next: (res) => {
          const term = this.genreSearchName.trim().toLowerCase();
          this.genres = (res || []).filter((g: any) =>
            g.name?.toLowerCase().includes(term)
          );
          if (!this.genres.length) this.toastr.info('No genres matched');
          this.cdr.detectChanges();
        },
        error: () => this.toastr.error('Search failed')
      });
      return;
    }
    this.loadGenres();
  }

  resetSearch() { this.searchId = null; this.genreSearchName = ''; this.loadGenres(); }

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
    // Start from full list, then apply filters client-side
    this.inventoryService.getAll().subscribe({
      next: (res) => {
        let list: any[] = res || [];

        if (this.inventorySearchId) {
          list = list.filter((i: any) => i.id === Number(this.inventorySearchId));
        }
        if (this.inventorySearchMovieId) {
          list = list.filter((i: any) => i.movieId === Number(this.inventorySearchMovieId));
        }
        if (this.inventorySearchMovieName.trim()) {
          const term = this.inventorySearchMovieName.trim().toLowerCase();
          list = list.filter((i: any) => i.movieName?.toLowerCase().includes(term));
        }
        if (this.inventoryStatusFilter === 'available') {
          list = list.filter((i: any) => i.isAvailable === true);
        } else if (this.inventoryStatusFilter === 'unavailable') {
          list = list.filter((i: any) => i.isAvailable === false);
        }

        this.inventories = list;
        if (!list.length) this.toastr.info('No inventory items matched');
        this.cdr.detectChanges();
      },
      error: () => this.toastr.error('Search failed')
    });
  }

  resetInventory() {
    this.inventorySearchId = null;
    this.inventorySearchMovieId = null;
    this.inventorySearchMovieName = '';
    this.inventoryStatusFilter = '';
    this.loadInventory();
  }

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
      next: (res) => {
        this.movies = [...res];
        const langs = new Set<string>();
        this.movies.forEach((m: any) => { if (m.language) langs.add(m.language); });
        this.movieLanguages = Array.from(langs).sort();
        this.cdr.detectChanges();
      },
      error: () => this.toastr.error('Failed to load movies')
    });
  }

  refreshMovies() { this.loadMovies(); this.toastr.success('Refreshed'); }

  searchMovie() {
    // Fetch all then apply filters + sort client-side
    this.movieService.getAll().subscribe({
      next: (res) => {
        let list: any[] = Array.isArray(res) ? res : [];

        if (this.movieSearchId) {
          list = list.filter((m: any) => m.id === Number(this.movieSearchId));
        }
        if (this.movieSearchName.trim()) {
          const term = this.movieSearchName.trim().toLowerCase();
          list = list.filter((m: any) => m.title?.toLowerCase().includes(term));
        }
        if (this.movieSearchLanguage.trim()) {
          const lang = this.movieSearchLanguage.trim().toLowerCase();
          list = list.filter((m: any) => m.language?.toLowerCase().includes(lang));
        }
        if (this.movieMinYear) {
          list = list.filter((m: any) => Number(m.releaseYear) >= Number(this.movieMinYear));
        }
        if (this.movieMaxYear) {
          list = list.filter((m: any) => Number(m.releaseYear) <= Number(this.movieMaxYear));
        }
        if (this.movieSort === 'rentalCount_desc') {
          list = list.sort((a, b) => (b.rentalCount ?? 0) - (a.rentalCount ?? 0));
        } else if (this.movieSort === 'rentalCount_asc') {
          list = list.sort((a, b) => (a.rentalCount ?? 0) - (b.rentalCount ?? 0));
        } else if (this.movieSort === 'rating_desc') {
          list = list.sort((a, b) => (b.rating ?? 0) - (a.rating ?? 0));
        } else if (this.movieSort === 'rating_asc') {
          list = list.sort((a, b) => (a.rating ?? 0) - (b.rating ?? 0));
        }
        this.movies = list;
        if (!list.length) this.toastr.info('No movies matched');
        this.cdr.detectChanges();
      },
      error: () => this.toastr.error('Search failed')
    });
  }

  resetMovie() {
    this.movieSearchId = null;
    this.movieSearchName = '';
    this.movieSearchLanguage = '';
    this.movieMinYear = null;
    this.movieMaxYear = null;
    this.movieSort = '';
    this.loadMovies();
  }

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

  onRentalDateFromChange(val: string) {
    // If From > To, clear To so user must re-pick
    if (val && this.rentalDateTo && val > this.rentalDateTo) {
      this.rentalDateTo = '';
      this.toastr.info('"From" date cannot be after "To" date');
    }
    this.rentalDateFrom = val;
  }

  onRentalDateToChange(val: string) {
    // If To < From, clear From
    if (val && this.rentalDateFrom && val < this.rentalDateFrom) {
      this.rentalDateFrom = '';
      this.toastr.info('"To" date cannot be before "From" date');
    }
    this.rentalDateTo = val;
  }

  searchRental() {
    this.rentalService.getAll().subscribe({
      next: (res: any) => {
        let list: any[] = Array.isArray(res) ? res : (res?.data || res?.items || []);

        if (this.rentalSearchUserId) {
          list = list.filter((r: any) => r.userId === Number(this.rentalSearchUserId));
        }
        if (this.rentalStatusFilter !== '') {
          list = list.filter((r: any) => r.status === Number(this.rentalStatusFilter));
        }
        if (this.rentalDateFrom) {
          const from = new Date(this.rentalDateFrom);
          list = list.filter((r: any) => new Date(r.rentalDate) >= from);
        }
        if (this.rentalDateTo) {
          const to = new Date(this.rentalDateTo);
          to.setHours(23, 59, 59);
          list = list.filter((r: any) => new Date(r.rentalDate) <= to);
        }
        if (this.rentalSortDate === 'desc') {
          list = list.sort((a, b) => new Date(b.rentalDate).getTime() - new Date(a.rentalDate).getTime());
        } else if (this.rentalSortDate === 'asc') {
          list = list.sort((a, b) => new Date(a.rentalDate).getTime() - new Date(b.rentalDate).getTime());
        }

        this.rentals = list;
        if (!list.length) this.toastr.info('No rentals matched');
        this.cdr.detectChanges();
      },
      error: () => this.toastr.error('Search failed')
    });
  }

  resetRental() {
    this.rentalSearchUserId = null;
    this.rentalDateFrom = '';
    this.rentalDateTo = '';
    this.rentalStatusFilter = '';
    this.rentalSortDate = '';
    this.loadRentals();
  }

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

  onPaymentDateFromChange(val: string) {
    if (val && this.paymentDateTo && val > this.paymentDateTo) {
      this.paymentDateTo = '';
      this.toastr.info('"From" date cannot be after "To" date');
    }
    this.paymentDateFrom = val;
  }

  onPaymentDateToChange(val: string) {
    if (val && this.paymentDateFrom && val < this.paymentDateFrom) {
      this.paymentDateFrom = '';
      this.toastr.info('"To" date cannot be before "From" date');
    }
    this.paymentDateTo = val;
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
    this.paymentService.getAll().subscribe({
      next: (res: any) => {
        let list: any[] = Array.isArray(res) ? res : (res?.data || []);

        if (this.paymentSearchRentalId) {
          list = list.filter((p: any) => p.rentalId === Number(this.paymentSearchRentalId));
        }
        if (this.paymentSearchUserId) {
          list = list.filter((p: any) => p.userId === Number(this.paymentSearchUserId));
        }
        if (this.paymentMethodFilter !== '') {
          list = list.filter((p: any) => (p.method ?? p.paymentMethod) === Number(this.paymentMethodFilter));
        }
        if (this.paymentStatusFilter !== '') {
          list = list.filter((p: any) => (p.status ?? p.paymentStatus) === Number(this.paymentStatusFilter));
        }
        if (this.paymentDateFrom) {
          const from = new Date(this.paymentDateFrom);
          list = list.filter((p: any) => new Date(p.paymentDate) >= from);
        }
        if (this.paymentDateTo) {
          const to = new Date(this.paymentDateTo);
          to.setHours(23, 59, 59);
          list = list.filter((p: any) => new Date(p.paymentDate) <= to);
        }
        if (this.paymentSortDate === 'desc') {
          list = list.sort((a, b) => new Date(b.paymentDate).getTime() - new Date(a.paymentDate).getTime());
        } else if (this.paymentSortDate === 'asc') {
          list = list.sort((a, b) => new Date(a.paymentDate).getTime() - new Date(b.paymentDate).getTime());
        }

        this.payments = list;
        if (!list.length) this.toastr.info('No payments matched');
        this.cdr.detectChanges();
      },
      error: () => this.toastr.error('Search failed')
    });
  }

  resetPayment() {
    this.paymentSearchRentalId = null;
    this.paymentSearchUserId = null;
    this.paymentMethodFilter = '';
    this.paymentStatusFilter = '';
    this.paymentDateFrom = '';
    this.paymentDateTo = '';
    this.paymentSortDate = '';
    this.loadPayments();
  }

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

  // ================= BROADCAST =================
  loadBroadcasts() {
    this.broadcastsLoading = true;
    this.notifService.getBroadcasts().subscribe({
      next: (res) => {
        this.broadcasts = res || [];
        this.filteredBroadcasts = [...this.broadcasts];
        this.broadcastsLoading = false;
        this.cdr.detectChanges();
      },
      error: () => { this.broadcastsLoading = false; this.cdr.detectChanges(); }
    });
  }

  applyBroadcastFilters() {
    let list = [...this.broadcasts];

    if (this.broadcastFilterUsername.trim()) {
      const term = this.broadcastFilterUsername.trim().toLowerCase();
      list = list.filter(b => b.sentByUsername?.toLowerCase().includes(term));
    }
    if (this.broadcastDateFrom) {
      const from = new Date(this.broadcastDateFrom);
      list = list.filter(b => new Date(b.sentAt) >= from);
    }
    if (this.broadcastDateTo) {
      const to = new Date(this.broadcastDateTo);
      to.setHours(23, 59, 59);
      list = list.filter(b => new Date(b.sentAt) <= to);
    }
    if (this.broadcastSortDate === 'desc') {
      list = list.sort((a, b) => new Date(b.sentAt).getTime() - new Date(a.sentAt).getTime());
    } else if (this.broadcastSortDate === 'asc') {
      list = list.sort((a, b) => new Date(a.sentAt).getTime() - new Date(b.sentAt).getTime());
    }

    this.filteredBroadcasts = list;
    this.cdr.detectChanges();
  }

  resetBroadcastFilters() {
    this.broadcastFilterUsername = '';
    this.broadcastDateFrom = '';
    this.broadcastDateTo = '';
    this.broadcastSortDate = '';
    this.filteredBroadcasts = [...this.broadcasts];
    this.cdr.detectChanges();
  }

  onBroadcastDateFromChange(val: string) {
    if (val && this.broadcastDateTo && val > this.broadcastDateTo) {
      this.broadcastDateTo = '';
      this.toastr.info('"From" date cannot be after "To" date');
    }
    this.broadcastDateFrom = val;
  }

  onBroadcastDateToChange(val: string) {
    if (val && this.broadcastDateFrom && val < this.broadcastDateFrom) {
      this.broadcastDateFrom = '';
      this.toastr.info('"To" date cannot be before "From" date');
    }
    this.broadcastDateTo = val;
  }

  sendBroadcast() {
    if (!this.broadcastTitle.trim() || !this.broadcastMessage.trim()) {
      this.toastr.error('Title and message are required');
      return;
    }
    const adminId = this.currentUser.currentUserId || this.currentUser.decodedUserId;
    this.notifService.broadcast(adminId, this.broadcastTitle, this.broadcastMessage).subscribe({
      next: (record) => {
        this.toastr.success('Notification sent to all customers');
        this.broadcastTitle = '';
        this.broadcastMessage = '';
        this.broadcasts = [record, ...this.broadcasts];
        this.filteredBroadcasts = [record, ...this.filteredBroadcasts];
        this.cdr.detectChanges();
      },
      error: () => this.toastr.error('Failed to send notification')
    });
  }

  deleteBroadcast(id: number) {
    if (!confirm('Delete this broadcast record?')) return;
    this.notifService.deleteBroadcast(id).subscribe({
      next: () => {
        this.broadcasts = this.broadcasts.filter(b => b.id !== id);
        this.filteredBroadcasts = this.filteredBroadcasts.filter(b => b.id !== id);
        this.toastr.success('Deleted');
        this.cdr.detectChanges();
      },
      error: () => this.toastr.error('Delete failed')
    });
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
