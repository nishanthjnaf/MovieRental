import { ChangeDetectorRef, Component, OnInit } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { CommonModule } from '@angular/common';
import { GenreService } from '../services/genre';
import { ToastrService } from 'ngx-toastr';
import { InventoryService } from '../services/inventory';
import { MovieService } from '../services/movie';
import { SeriesService } from '../services/series';
import { RentalService } from '../services/rental';
import { PaymentService } from '../services/payment';
import { Router } from '@angular/router';
import { CurrentUserService } from '../services/current-user';
import { UserService } from '../services/user';
import { NotificationService } from '../services/notification';
import { ActivityLogService } from '../services/activity-log';
import { catchError, forkJoin, of } from 'rxjs';
import { ThemeToggle } from '../components/theme-toggle';
import { SafeUrlPipe } from '../pipes/safe-url.pipe';

@Component({
  selector: 'app-admin',
  standalone: true,
  templateUrl: './admin.html',
  styleUrl: './admin.css',
  imports: [CommonModule, FormsModule, ThemeToggle, SafeUrlPipe]
})
export class Admin implements OnInit {

  constructor(
    private genreService: GenreService,
    private inventoryService: InventoryService,
    private movieService: MovieService,
    private seriesService: SeriesService,
    private rentalService: RentalService,
    private paymentService: PaymentService,
    private userService: UserService,
    private currentUser: CurrentUserService,
    private notifService: NotificationService,
    private activityLogService: ActivityLogService,
    private router: Router,
    private toastr: ToastrService,
    private cdr: ChangeDetectorRef
  ) {}

  viewMode: 'services' | 'genre' | 'inventory' | 'movie' | 'series' | 'rental' | 'payment' | 'broadcast' | 'logs' = 'services';

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
  allGenres: any[] = [];
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
  private allInventories: any[] = [];
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
  private allMovies: any[] = [];
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
  movieFormGenreIds: number[] = [];
  genreDropdownOpen = false;

  get selectedGenreNames(): string {
    if (!this.movieFormGenreIds.length) return 'Select genres...';
    return this.allGenres
      .filter(g => this.movieFormGenreIds.includes(g.id))
      .map(g => g.name)
      .join(', ');
  }
  showPoster = '';
  showTrailer = '';

  // ---- RENTAL ----
  rentals: any[] = [];
  private allRentals: any[] = [];
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
  private allPayments: any[] = [];
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

  // ---- LOGS ----
  logs: any[] = [];
  logsLoading = false;
  logsTotalCount = 0;
  logsPage = 1;
  logsPageSize = 50;
  logFilterUserId: any;
  logFilterRole = '';
  logFilterEntity = '';
  logFilterAction = '';
  logFilterStatus = '';
  logFilterFrom = '';
  logFilterTo = '';
  logSortOrder = 'desc';

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
    if (service === 'series') this.loadSeries();
    if (service === 'rental') this.loadRentals();
    if (service === 'payment') this.loadPayments();
    if (service === 'broadcast') this.loadBroadcasts();
    if (service === 'logs') this.loadLogs();
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

  getPageTitle(): string {
    if (this.viewMode === 'services') return 'Dashboard';
    const map: Record<string, string> = {
      genre: 'Genre', inventory: 'Inventory', movie: 'Movie', series: 'Series',
      rental: 'Rental', payment: 'Payment', broadcast: 'Broadcast', logs: 'Activity Logs'
    };
    return (map[this.viewMode] ?? this.viewMode) + ' Management';
  }

  // ================= GENRE =================
  loadGenres() {
    this.genreService.getAllGenres().subscribe({
      next: (res) => { this.allGenres = [...(res || [])]; this.genres = [...this.allGenres]; this.cdr.detectChanges(); },
      error: () => this.toastr.error('Failed to load genres')
    });
  }

  refreshGenres() { this.loadGenres(); this.toastr.success('Refreshed'); }

  searchGenre() {
    let list = [...this.allGenres];
    if (this.searchId) {
      list = list.filter((g: any) => g.id === Number(this.searchId));
    }
    if (this.genreSearchName.trim()) {
      const term = this.genreSearchName.trim().toLowerCase();
      list = list.filter((g: any) => g.name?.toLowerCase().includes(term));
    }
    this.genres = list;
    this.cdr.detectChanges();
  }

  resetSearch() { this.searchId = null; this.genreSearchName = ''; this.genres = [...this.allGenres]; this.cdr.detectChanges(); }

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
      next: (res) => { this.allInventories = [...(res || [])]; this.inventories = [...this.allInventories]; this.cdr.detectChanges(); },
      error: () => this.toastr.error('Failed to load inventory')
    });
  }

  refreshInventory() { this.loadInventory(); this.toastr.success('Refreshed'); }

  searchInventory() {
    let list = [...this.allInventories];
    if (this.inventorySearchId) list = list.filter((i: any) => i.id === Number(this.inventorySearchId));
    if (this.inventorySearchMovieId) list = list.filter((i: any) => i.movieId === Number(this.inventorySearchMovieId));
    if (this.inventorySearchMovieName.trim()) {
      const term = this.inventorySearchMovieName.trim().toLowerCase();
      list = list.filter((i: any) => i.movieName?.toLowerCase().includes(term));
    }
    if (this.inventoryStatusFilter === 'available') list = list.filter((i: any) => i.isAvailable === true);
    else if (this.inventoryStatusFilter === 'unavailable') list = list.filter((i: any) => i.isAvailable === false);
    this.inventories = list;
    this.cdr.detectChanges();
  }

  resetInventory() {
    this.inventorySearchId = null; this.inventorySearchMovieId = null;
    this.inventorySearchMovieName = ''; this.inventoryStatusFilter = '';
    this.inventories = [...this.allInventories]; this.cdr.detectChanges();
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
        this.allMovies = Array.isArray(res) ? res : [];
        this.movies = [...this.allMovies];
        const langs = new Set<string>();
        this.allMovies.forEach((m: any) => { if (m.language) langs.add(m.language); });
        this.movieLanguages = Array.from(langs).sort();
        this.cdr.detectChanges();
      },
      error: () => this.toastr.error('Failed to load movies')
    });
  }

  refreshMovies() { this.loadMovies(); this.toastr.success('Refreshed'); }

  searchMovie() {
    let list = [...this.allMovies];
    if (this.movieSearchId) list = list.filter((m: any) => m.id === Number(this.movieSearchId));
    if (this.movieSearchName.trim()) {
      const term = this.movieSearchName.trim().toLowerCase();
      list = list.filter((m: any) => m.title?.toLowerCase().includes(term));
    }
    if (this.movieSearchLanguage.trim()) list = list.filter((m: any) => m.language === this.movieSearchLanguage);
    if (this.movieMinYear) list = list.filter((m: any) => Number(m.releaseYear) >= Number(this.movieMinYear));
    if (this.movieMaxYear) list = list.filter((m: any) => Number(m.releaseYear) <= Number(this.movieMaxYear));
    if (this.movieSort === 'rentalCount_desc') list.sort((a, b) => (b.rentalCount ?? 0) - (a.rentalCount ?? 0));
    else if (this.movieSort === 'rentalCount_asc') list.sort((a, b) => (a.rentalCount ?? 0) - (b.rentalCount ?? 0));
    else if (this.movieSort === 'rating_desc') list.sort((a, b) => (b.rating ?? 0) - (a.rating ?? 0));
    else if (this.movieSort === 'rating_asc') list.sort((a, b) => (a.rating ?? 0) - (b.rating ?? 0));
    this.movies = list;
    this.cdr.detectChanges();
  }

  resetMovie() {
    this.movieSearchId = null; this.movieSearchName = ''; this.movieSearchLanguage = '';
    this.movieMinYear = null; this.movieMaxYear = null; this.movieSort = '';
    this.movies = [...this.allMovies]; this.cdr.detectChanges();
  }

  openMovieForm() {
    this.isMovieEdit = false;
    this.movieFormGenreIds = [];
    this.movieForm = {
      title: '', description: '', releaseYear: 2024, durationMinutes: 0,
      language: '', director: '', cast: '', contentRating: '',
      contentAdvisory: '', posterPath: '', trailerUrl: ''
    };
    this.ensureGenresLoaded(() => { this.showMovieForm = true; this.cdr.detectChanges(); });
  }

  editMovie(m: any) {
    this.isMovieEdit = true;
    this.selectedMovieId = m.id;
    this.movieFormGenreIds = Array.isArray(m.genreIds) ? [...m.genreIds] : [];
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
    this.ensureGenresLoaded(() => { this.showMovieForm = true; this.cdr.detectChanges(); });
  }

  private ensureGenresLoaded(callback: () => void) {
    if (this.allGenres.length > 0) { callback(); return; }
    this.genreService.getAllGenres().subscribe({
      next: (res) => { this.allGenres = res || []; this.genres = [...this.allGenres]; callback(); },
      error: () => callback()
    });
  }

  toggleMovieGenre(genreId: number) {
    const idx = this.movieFormGenreIds.indexOf(genreId);
    if (idx === -1) this.movieFormGenreIds.push(genreId);
    else this.movieFormGenreIds.splice(idx, 1);
  }

  isMovieGenreSelected(genreId: number): boolean {
    return this.movieFormGenreIds.includes(genreId);
  }

  saveMovie() {
    const payload = { ...this.movieForm, genreIds: this.movieFormGenreIds };
    if (this.isMovieEdit && this.selectedMovieId !== null) {
      this.movieService.update(this.selectedMovieId, payload).subscribe({
        next: () => { this.toastr.success('Updated'); this.showMovieForm = false; this.loadMovies(); },
        error: () => this.toastr.error('Update failed')
      });
    } else {
      this.movieService.add(payload).subscribe({
        next: () => { this.toastr.success('Added'); this.showMovieForm = false; this.loadMovies(); },
        error: () => this.toastr.error('Add failed')
      });
    }
  }

  cancelMovieForm() { this.showMovieForm = false; this.genreDropdownOpen = false; this.cdr.detectChanges(); }

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
        this.allRentals = Array.isArray(res) ? res : (res?.data || res?.items || []);
        this.rentals = [...this.allRentals];
        this.cdr.detectChanges();
      },
      error: () => this.toastr.error('Failed to load rentals')
    });
  }

  refreshRentals() { this.loadRentals(); this.toastr.success('Refreshed'); }

  onRentalDateFromChange(val: string) {
    if (val && this.rentalDateTo && val > this.rentalDateTo) { this.rentalDateTo = ''; }
    this.rentalDateFrom = val;
  }
  onRentalDateToChange(val: string) {
    if (val && this.rentalDateFrom && val < this.rentalDateFrom) { this.rentalDateFrom = ''; }
    this.rentalDateTo = val;
  }

  searchRental() {
    let list = [...this.allRentals];
    if (this.rentalSearchUserId) list = list.filter((r: any) => r.userId === Number(this.rentalSearchUserId));
    if (this.rentalStatusFilter !== '') list = list.filter((r: any) => r.status === Number(this.rentalStatusFilter));
    if (this.rentalDateFrom) { const from = new Date(this.rentalDateFrom); list = list.filter((r: any) => new Date(r.rentalDate) >= from); }
    if (this.rentalDateTo) { const to = new Date(this.rentalDateTo); to.setHours(23,59,59); list = list.filter((r: any) => new Date(r.rentalDate) <= to); }
    if (this.rentalSortDate === 'desc') list.sort((a, b) => new Date(b.rentalDate).getTime() - new Date(a.rentalDate).getTime());
    else if (this.rentalSortDate === 'asc') list.sort((a, b) => new Date(a.rentalDate).getTime() - new Date(b.rentalDate).getTime());
    this.rentals = list;
    this.cdr.detectChanges();
  }

  resetRental() {
    this.rentalSearchUserId = null; this.rentalDateFrom = ''; this.rentalDateTo = '';
    this.rentalStatusFilter = ''; this.rentalSortDate = '';
    this.rentals = [...this.allRentals]; this.cdr.detectChanges();
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
        this.allPayments = Array.isArray(res) ? res : (res?.data || []);
        this.payments = [...this.allPayments];
        this.cdr.detectChanges();
      },
      error: () => this.toastr.error('Failed to load payments')
    });
  }

  refreshPayments() { this.loadPayments(); this.toastr.success('Refreshed'); }

  searchPayment() {
    let list = [...this.allPayments];
    if (this.paymentSearchRentalId) list = list.filter((p: any) => p.rentalId === Number(this.paymentSearchRentalId));
    if (this.paymentSearchUserId) list = list.filter((p: any) => p.userId === Number(this.paymentSearchUserId));
    if (this.paymentMethodFilter !== '') list = list.filter((p: any) => (p.method ?? p.paymentMethod) === Number(this.paymentMethodFilter));
    if (this.paymentStatusFilter !== '') list = list.filter((p: any) => (p.status ?? p.paymentStatus) === Number(this.paymentStatusFilter));
    if (this.paymentDateFrom) { const from = new Date(this.paymentDateFrom); list = list.filter((p: any) => new Date(p.paymentDate) >= from); }
    if (this.paymentDateTo) { const to = new Date(this.paymentDateTo); to.setHours(23,59,59); list = list.filter((p: any) => new Date(p.paymentDate) <= to); }
    if (this.paymentSortDate === 'desc') list.sort((a, b) => new Date(b.paymentDate).getTime() - new Date(a.paymentDate).getTime());
    else if (this.paymentSortDate === 'asc') list.sort((a, b) => new Date(a.paymentDate).getTime() - new Date(b.paymentDate).getTime());
    this.payments = list;
    this.cdr.detectChanges();
  }

  resetPayment() {
    this.paymentSearchRentalId = null; this.paymentSearchUserId = null;
    this.paymentMethodFilter = ''; this.paymentStatusFilter = '';
    this.paymentDateFrom = ''; this.paymentDateTo = ''; this.paymentSortDate = '';
    this.payments = [...this.allPayments]; this.cdr.detectChanges();
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

  // ================= ACTIVITY LOGS =================
  loadLogs() {
    this.logsLoading = true;
    this.activityLogService.getLogs({
      userId: this.logFilterUserId || undefined,
      role: this.logFilterRole || undefined,
      entity: this.logFilterEntity || undefined,
      action: this.logFilterAction || undefined,
      status: this.logFilterStatus || undefined,
      from: this.logFilterFrom || undefined,
      to: this.logFilterTo || undefined,
      sortOrder: this.logSortOrder,
      page: this.logsPage,
      pageSize: this.logsPageSize
    }).pipe(catchError(() => of(null))).subscribe({
      next: (res: any) => {
        this.logs = res?.items || [];
        this.logsTotalCount = res?.totalCount || 0;
        this.logsLoading = false;
        this.cdr.detectChanges();
      },
      error: () => { this.logsLoading = false; this.cdr.detectChanges(); }
    });
  }

  searchLogs() { this.logsPage = 1; this.loadLogs(); }

  resetLogs() {
    this.logFilterUserId = null;
    this.logFilterRole = '';
    this.logFilterEntity = '';
    this.logFilterAction = '';
    this.logFilterStatus = '';
    this.logFilterFrom = '';
    this.logFilterTo = '';
    this.logSortOrder = 'desc';
    this.logsPage = 1;
    this.loadLogs();
  }

  logsNextPage() {
    if (this.logsPage * this.logsPageSize < this.logsTotalCount) {
      this.logsPage++;
      this.loadLogs();
    }
  }

  logsPrevPage() {
    if (this.logsPage > 1) {
      this.logsPage--;
      this.loadLogs();
    }
  }

  onLogDateFromChange(val: string) {
    if (val && this.logFilterTo && val > this.logFilterTo) { this.logFilterTo = ''; }
    this.logFilterFrom = val;
  }

  onLogDateToChange(val: string) {
    if (val && this.logFilterFrom && val < this.logFilterFrom) { this.logFilterFrom = ''; }
    this.logFilterTo = val;
  }

  get logsTotalPages(): number {
    return Math.ceil(this.logsTotalCount / this.logsPageSize) || 1;
  }

  // ================= SERIES =================
  seriesList: any[] = [];
  private allSeriesList: any[] = [];
  seriesSearchName = '';
  seriesFilterLanguage = '';
  seriesFilterAvailable = '';
  seriesMinPrice: any;
  seriesMaxPrice: any;
  seriesSortOption = '';
  seriesLanguages: string[] = [];
  seriesExpandedId: number | null = null;
  seriesExpandedSeasonId: number | null = null;
  showSeriesForm = false;
  isSeriesEdit = false;
  selectedSeriesId: number | null = null;
  seriesFormStep: 'details' | 'seasons' = 'details';
  seriesForm = { title: '', description: '', language: '', director: '', cast: '', contentRating: '', contentAdvisory: '', posterPath: '', trailerUrl: '', rentalPrice: 0, isAvailable: true };
  seriesFormGenreIds: number[] = [];
  seriesGenreDropdownOpen = false;
  seriesSeasonCount = 1;
  seriesSeasons: Array<{ seasonNumber: number; title: string; releaseYear: number; episodeCount: number; episodes: Array<{ episodeNumber: number; title: string; description: string; durationMinutes: number }>; }> = [];
  showEditSeasonModal = false;
  editingSeasonData: any = null;
  showEditEpisodeModal = false;
  editingEpisodeData: any = null;
  editingEpisodeSeasonId: number | null = null;

  get seriesSelectedGenreNames(): string {
    if (!this.seriesFormGenreIds.length) return 'Select genres...';
    return this.allGenres.filter(g => this.seriesFormGenreIds.includes(g.id)).map(g => g.name).join(', ');
  }

  loadSeries() {
    this.seriesService.getAll().subscribe({
      next: (res) => {
        this.allSeriesList = res || [];
        this.seriesLanguages = [...new Set(this.allSeriesList.map(s => s.language).filter(Boolean))].sort() as string[];
        this.seriesList = [...this.allSeriesList];
        this.cdr.detectChanges();
      },
      error: () => this.toastr.error('Failed to load series')
    });
  }

  searchSeries() {
    let list = [...this.allSeriesList];
    if (this.seriesSearchName.trim()) {
      const q = this.seriesSearchName.trim().toLowerCase();
      list = list.filter(s => s.title?.toLowerCase().includes(q) || s.director?.toLowerCase().includes(q));
    }
    if (this.seriesFilterLanguage) list = list.filter(s => s.language === this.seriesFilterLanguage);
    if (this.seriesFilterAvailable === 'true') list = list.filter(s => s.isAvailable === true);
    else if (this.seriesFilterAvailable === 'false') list = list.filter(s => s.isAvailable === false);
    if (this.seriesMinPrice != null && this.seriesMinPrice !== '') list = list.filter(s => s.rentalPrice >= Number(this.seriesMinPrice));
    if (this.seriesMaxPrice != null && this.seriesMaxPrice !== '') list = list.filter(s => s.rentalPrice <= Number(this.seriesMaxPrice));
    if (this.seriesSortOption === 'price-asc') list.sort((a, b) => a.rentalPrice - b.rentalPrice);
    else if (this.seriesSortOption === 'price-desc') list.sort((a, b) => b.rentalPrice - a.rentalPrice);
    else if (this.seriesSortOption === 'rentals-desc') list.sort((a, b) => (b.rentalCount ?? 0) - (a.rentalCount ?? 0));
    else if (this.seriesSortOption === 'rentals-asc') list.sort((a, b) => (a.rentalCount ?? 0) - (b.rentalCount ?? 0));
    else if (this.seriesSortOption === 'seasons-desc') list.sort((a, b) => (b.seasons?.length ?? 0) - (a.seasons?.length ?? 0));
    else if (this.seriesSortOption === 'seasons-asc') list.sort((a, b) => (a.seasons?.length ?? 0) - (b.seasons?.length ?? 0));
    this.seriesList = list;
    this.cdr.detectChanges();
  }

  resetSeries() {
    this.seriesSearchName = ''; this.seriesFilterLanguage = ''; this.seriesFilterAvailable = '';
    this.seriesMinPrice = null; this.seriesMaxPrice = null; this.seriesSortOption = '';
    this.seriesList = [...this.allSeriesList]; this.cdr.detectChanges();
  }
  toggleSeriesRow(id: number) { this.seriesExpandedId = this.seriesExpandedId === id ? null : id; this.seriesExpandedSeasonId = null; this.cdr.detectChanges(); }
  toggleSeasonRow(id: number) { this.seriesExpandedSeasonId = this.seriesExpandedSeasonId === id ? null : id; this.cdr.detectChanges(); }

  openSeriesForm() {
    this.isSeriesEdit = false; this.selectedSeriesId = null; this.seriesFormStep = 'details';
    this.seriesForm = { title: '', description: '', language: '', director: '', cast: '', contentRating: '', contentAdvisory: '', posterPath: '', trailerUrl: '', rentalPrice: 0, isAvailable: true };
    this.seriesFormGenreIds = []; this.seriesSeasonCount = 1; this.seriesSeasons = [];
    this.ensureGenresLoaded(() => { this.showSeriesForm = true; this.cdr.detectChanges(); });
  }

  editSeries(s: any) {
    this.isSeriesEdit = true; this.selectedSeriesId = s.id; this.seriesFormStep = 'details';
    this.seriesForm = { title: s.title ?? '', description: s.description ?? '', language: s.language ?? '', director: s.director ?? '', cast: Array.isArray(s.cast) ? s.cast.join(', ') : (s.cast ?? ''), contentRating: s.contentRating ?? '', contentAdvisory: Array.isArray(s.contentAdvisory) ? s.contentAdvisory.join(', ') : (s.contentAdvisory ?? ''), posterPath: s.posterPath ?? '', trailerUrl: s.trailerUrl ?? '', rentalPrice: s.rentalPrice ?? 0, isAvailable: s.isAvailable ?? true };
    this.ensureGenresLoaded(() => {
      const names: string[] = Array.isArray(s.genres) ? s.genres : [];
      this.seriesFormGenreIds = this.allGenres.filter(g => names.includes(g.name)).map(g => g.id);
      this.showSeriesForm = true; this.cdr.detectChanges();
    });
  }

  seriesDetailsNext() {
    if (!this.seriesForm.title.trim()) { this.toastr.error('Title is required'); return; }
    if (this.isSeriesEdit) { this.saveSeriesDetails(); return; }
    this.seriesFormStep = 'seasons'; this.buildSeasonForms(); this.cdr.detectChanges();
  }

  buildSeasonForms() {
    const count = Math.max(1, Math.min(20, Number(this.seriesSeasonCount) || 1));
    this.seriesSeasons = Array.from({ length: count }, (_, i) => this.seriesSeasons[i] ?? { seasonNumber: i + 1, title: `Season ${i + 1}`, releaseYear: new Date().getFullYear(), episodeCount: 1, episodes: [{ episodeNumber: 1, title: 'Episode 1', description: '', durationMinutes: 45 }] });
  }

  buildEpisodeForms(idx: number) {
    const s = this.seriesSeasons[idx];
    const count = Math.max(1, Math.min(50, Number(s.episodeCount) || 1));
    s.episodes = Array.from({ length: count }, (_, i) => s.episodes[i] ?? { episodeNumber: i + 1, title: `Episode ${i + 1}`, description: '', durationMinutes: 45 });
    this.cdr.detectChanges();
  }

  saveSeriesDetails() {
    const existing = this.allSeriesList.find(x => x.id === this.selectedSeriesId);
    const payload = { ...this.seriesForm, genreIds: this.seriesFormGenreIds, seasons: existing?.seasons?.map((sn: any) => ({ ...sn, episodes: sn.episodes })) ?? [] };
    this.seriesService.update(this.selectedSeriesId!, payload).subscribe({
      next: () => { this.toastr.success('Series updated'); this.showSeriesForm = false; this.loadSeries(); },
      error: () => this.toastr.error('Update failed')
    });
  }

  saveSeries() {
    const payload = { ...this.seriesForm, genreIds: this.seriesFormGenreIds, seasons: this.seriesSeasons.map(s => ({ seasonNumber: s.seasonNumber, title: s.title, releaseYear: s.releaseYear, episodes: s.episodes })) };
    this.seriesService.add(payload).subscribe({
      next: () => { this.toastr.success('Series added'); this.showSeriesForm = false; this.loadSeries(); },
      error: (err) => this.toastr.error(err?.error || 'Add failed')
    });
  }

  cancelSeriesForm() { this.showSeriesForm = false; this.seriesGenreDropdownOpen = false; this.cdr.detectChanges(); }

  deleteSeries(id: number) {
    if (!confirm('Delete this series and all its seasons/episodes?')) return;
    this.seriesService.delete(id).subscribe({
      next: () => { this.toastr.success('Deleted'); this.loadSeries(); },
      error: () => { this.toastr.success('Deleted'); this.loadSeries(); }
    });
  }

  toggleSeriesGenre(id: number) { const idx = this.seriesFormGenreIds.indexOf(id); if (idx === -1) this.seriesFormGenreIds.push(id); else this.seriesFormGenreIds.splice(idx, 1); }
  isSeriesGenreSelected(id: number): boolean { return this.seriesFormGenreIds.includes(id); }

  openEditSeason(season: any) { this.editingSeasonData = { ...season }; this.showEditSeasonModal = true; this.cdr.detectChanges(); }

  saveEditSeason() {
    if (!this.editingSeasonData) return;
    const series = this.allSeriesList.find(s => s.seasons?.some((sn: any) => sn.id === this.editingSeasonData.id));
    if (!series) { this.toastr.error('Series not found'); return; }
    const genreIds = this.allGenres.filter(g => (series.genres || []).includes(g.name)).map((g: any) => g.id);
    const payload = { ...series, genreIds, seasons: series.seasons.map((sn: any) => sn.id === this.editingSeasonData.id ? { ...sn, title: this.editingSeasonData.title, releaseYear: this.editingSeasonData.releaseYear, episodes: sn.episodes } : { ...sn, episodes: sn.episodes }) };
    this.seriesService.update(series.id, payload).subscribe({
      next: () => { this.toastr.success('Season updated'); this.showEditSeasonModal = false; this.loadSeries(); },
      error: () => this.toastr.error('Update failed')
    });
  }

  openEditEpisode(episode: any, seasonId: number) { this.editingEpisodeData = { ...episode }; this.editingEpisodeSeasonId = seasonId; this.showEditEpisodeModal = true; this.cdr.detectChanges(); }

  saveEditEpisode() {
    if (!this.editingEpisodeData || !this.editingEpisodeSeasonId) return;
    const series = this.allSeriesList.find(s => s.seasons?.some((sn: any) => sn.id === this.editingEpisodeSeasonId));
    if (!series) { this.toastr.error('Series not found'); return; }
    const genreIds = this.allGenres.filter(g => (series.genres || []).includes(g.name)).map((g: any) => g.id);
    const payload = { ...series, genreIds, seasons: series.seasons.map((sn: any) => sn.id === this.editingEpisodeSeasonId ? { ...sn, episodes: sn.episodes.map((ep: any) => ep.id === this.editingEpisodeData.id ? { ...this.editingEpisodeData } : ep) } : { ...sn, episodes: sn.episodes }) };
    this.seriesService.update(series.id, payload).subscribe({
      next: () => { this.toastr.success('Episode updated'); this.showEditEpisodeModal = false; this.loadSeries(); },
      error: () => this.toastr.error('Update failed')
    });
  }

  // Add Season to existing series
  showAddSeasonModal = false;
  addSeasonSeriesId: number | null = null;
  addSeasonForm = { seasonNumber: 1, title: '', releaseYear: new Date().getFullYear(), episodeCount: 1, episodes: [] as Array<{ episodeNumber: number; title: string; description: string; durationMinutes: number }> };

  openAddSeasonModal(seriesId: number) {
    this.addSeasonSeriesId = seriesId;
    const series = this.allSeriesList.find(s => s.id === seriesId);
    const nextNum = (series?.seasons?.length ?? 0) + 1;
    this.addSeasonForm = { seasonNumber: nextNum, title: `Season ${nextNum}`, releaseYear: new Date().getFullYear(), episodeCount: 1, episodes: [{ episodeNumber: 1, title: 'Episode 1', description: '', durationMinutes: 45 }] };
    this.showAddSeasonModal = true;
    this.cdr.detectChanges();
  }

  buildAddSeasonEpisodes() {
    const count = Math.max(1, Math.min(50, Number(this.addSeasonForm.episodeCount) || 1));
    this.addSeasonForm.episodes = Array.from({ length: count }, (_, i) =>
      this.addSeasonForm.episodes[i] ?? { episodeNumber: i + 1, title: `Episode ${i + 1}`, description: '', durationMinutes: 45 }
    );
    this.cdr.detectChanges();
  }

  saveAddSeason() {
    if (!this.addSeasonSeriesId) return;
    const payload = {
      seriesId: this.addSeasonSeriesId,
      seasonNumber: this.addSeasonForm.seasonNumber,
      title: this.addSeasonForm.title,
      releaseYear: this.addSeasonForm.releaseYear,
      episodes: this.addSeasonForm.episodes
    };
    this.seriesService.addSeason(payload).subscribe({
      next: () => { this.toastr.success('Season added'); this.showAddSeasonModal = false; this.loadSeries(); },
      error: (err) => this.toastr.error(err?.error || 'Failed to add season')
    });
  }

  // Add Episode to existing season
  showAddEpisodeModal = false;
  addEpisodeSeasonId: number | null = null;
  addEpisodeSeriesTitle = '';
  addEpisodeSeasonTitle = '';
  addEpisodeForm = { episodeNumber: 1, title: '', description: '', durationMinutes: 45 };

  openAddEpisodeModal(season: any, seriesTitle: string) {
    this.addEpisodeSeasonId = season.id;
    this.addEpisodeSeriesTitle = seriesTitle;
    this.addEpisodeSeasonTitle = `Season ${season.seasonNumber}`;
    const nextNum = (season.episodes?.length ?? 0) + 1;
    this.addEpisodeForm = { episodeNumber: nextNum, title: `Episode ${nextNum}`, description: '', durationMinutes: 45 };
    this.showAddEpisodeModal = true;
    this.cdr.detectChanges();
  }

  saveAddEpisode() {
    if (!this.addEpisodeSeasonId) return;
    const payload = { seasonId: this.addEpisodeSeasonId, ...this.addEpisodeForm };
    this.seriesService.addEpisode(payload).subscribe({
      next: () => { this.toastr.success('Episode added'); this.showAddEpisodeModal = false; this.loadSeries(); },
      error: (err) => this.toastr.error(err?.error || 'Failed to add episode')
    });
  }

  // ================= AUTH =================
  logout() {    this.showProfileMenu = false;
    localStorage.removeItem('token');
    sessionStorage.removeItem('token');
    localStorage.removeItem('role');
    this.currentUser.clear();
    this.toastr.info('Logged out');
    this.router.navigate(['/login']);
  }
}
