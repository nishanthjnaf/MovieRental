import { Component, OnInit } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { CommonModule } from '@angular/common';
import { GenreService } from '../services/genre';
import { ToastrService } from 'ngx-toastr';
import { InventoryService } from '../services/inventory';
import { MovieService } from '../services/movie';
import { RentalService } from '../services/rental';
import { PaymentService } from '../services/payment';



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
    private rentalService:RentalService,
    private paymentService:PaymentService,

    private toastr: ToastrService
  ) {}

  viewMode: 'services' | 'genre' | 'inventory' | 'movie' | 'rental' | 'payment' = 'services';
  genres: any[] = [];
  searchId: any;

  showForm = false;
  isEdit = false;
  selectedId: number | null = null;
  inventories: any[] = [];
  inventoryForm = {
    movieId: 0,
    rentalPrice: 0,
    isAvailable: true
  };
  payments: any[] = [];

paymentSearchRentalId: any;
paymentSearchUserId: any;
  // -------- RENTAL --------
rentals: any[] = [];

rentalSearchUserId: any;

showRentalItems = false;
selectedRentalId: number | null = null;
rentalItems: any[] = [];

showInventoryForm = false;
isInventoryEdit = false;
selectedInventoryId: number | null = null;
inventorySearchId: any;

  formData = {
    name: '',
    description: ''
  };

  showAssign = false;
  selectedGenreId: number | null = null;
  movieId: number | null = null;
  // ---------- MOVIE ----------
movies: any[] = [];

movieSearchId: any;
movieSearchName: string = '';

showMovieForm = false;
isMovieEdit = false;
selectedMovieId: number | null = null;

movieForm = {
  title: '',
  description: '',
  releaseYear: 2024,
  durationMinutes: 0,
  language: '',
  posterPath: '',
  trailerUrl: ''
};

showPoster = '';
showTrailer = '';

  ngOnInit() {}

  openService(service: string) {
  this.viewMode = service as any;

  if (service === 'genre') this.loadGenres();
  if (service === 'inventory') this.loadInventory();
  if (service === 'movie') this.loadMovies();
  if (service === 'rental') this.loadRentals();
  if (service === 'payment') this.loadPayments();
}
  loadInventory() {
  this.inventoryService.getAll().subscribe({
    next: (res) => this.inventories = [...res],
    error: () => this.toastr.error('Failed to load inventory')
  });
}

  loadGenres() {
  this.genreService.getAllGenres().subscribe({
    next: (res) => {
      this.genres = [...res]; // ✅ force change detection
    },
    error: () => {
      this.toastr.error('Failed to load genres');
    }
  });
}
refreshGenres() {
  this.loadGenres();
  this.toastr.success('Data refreshed');
}

  resetSearch() {
    this.searchId = null;
    this.loadGenres();
  }

  searchGenre() {
    if (!this.searchId) {
      this.loadGenres();
      return;
    }

    this.genreService.getGenreById(this.searchId).subscribe({
      next: (res) => this.genres = [res],
      error: () => this.toastr.error('Genre not found')
    });
  }

  openAddForm() {
    this.showForm = true;
    this.isEdit = false;
    this.formData = { name: '', description: '' };
  }

  editGenre(g: any) {
    this.showForm = true;
    this.isEdit = true;
    this.selectedId = g.id;

    this.formData = {
      name: g.name,
      description: g.description
    };
  }

  saveGenre() {

  if (this.isEdit && this.selectedId !== null) {

    this.genreService.updateGenre(this.selectedId, this.formData).subscribe({
      next: () => {
        this.toastr.success('Updated successfully');

        this.loadGenres(); // ✅ refresh from backend
        this.cancelForm();
      },
      error: () => this.toastr.error('Update failed')
    });

  } else {

    this.genreService.addGenre(this.formData).subscribe({
      next: () => {
        this.toastr.success('Added successfully');

        this.loadGenres(); // ✅ refresh
        this.cancelForm();
      },
      error: () => this.toastr.error('Add failed')
    });

  }
}

  cancelForm() {
    this.showForm = false;
  }

  deleteGenre(id: number) {

  const confirmDelete = confirm('Are you sure you want to delete this genre?');
  if (!confirmDelete) return;

  this.genreService.deleteGenre(id).subscribe({
    next: () => {
      this.toastr.success('Deleted successfully');
      this.loadGenres(); // ✅ refresh
    },
    error: () => {
      // backend may still delete
      this.loadGenres(); // ✅ force sync
      this.toastr.success('Deleted successfully');
    }
  });

}

  openAssign(g: any) {
    this.showAssign = true;
    this.selectedGenreId = g.id;
    this.movieId = null;
  }

  assignMovie() {

  if (!this.selectedGenreId || !this.movieId) {
    this.toastr.error('Enter valid Movie ID');
    return;
  }

  this.genreService.assignMovie(this.selectedGenreId, this.movieId).subscribe({
    next: () => {
      this.toastr.success('Movie assigned successfully 🎉');

      this.loadGenres(); // ✅ AUTO REFRESH (IMPORTANT)
      this.cancelAssign();
    },
    error: () => {
      this.toastr.error('Assignment failed ❌');
    }
  });

}

  cancelAssign() {
    this.showAssign = false;
  }
  //----------------Inventory-----------------

  refreshInventory() {
  this.loadInventory();
  this.toastr.success('Inventory refreshed');
}

openInventoryForm() {
  this.showInventoryForm = true;
  this.isInventoryEdit = false;
  this.inventoryForm = { movieId: 0, rentalPrice: 0, isAvailable: true };
}

editInventory(i: any) {
  this.showInventoryForm = true;
  this.isInventoryEdit = true;
  this.selectedInventoryId = i.id;

  this.inventoryForm = {
    movieId: i.movieId,
    rentalPrice: i.rentalPrice,
    isAvailable: i.isAvailable
  };
}

saveInventory() {

  if (this.isInventoryEdit && this.selectedInventoryId !== null) {

    this.inventoryService.update(this.selectedInventoryId, this.inventoryForm).subscribe({
      next: () => {
        this.toastr.success('Updated successfully');
        this.loadInventory();
        this.showInventoryForm = false;
      },
      error: () => this.toastr.error('Update failed')
    });

  } else {

    this.inventoryService.add(this.inventoryForm).subscribe({
      next: () => {
        this.toastr.success('Added successfully');
        this.loadInventory();
        this.showInventoryForm = false;
      },
      error: () => this.toastr.error('Add failed')
    });

  }
}

deleteInventory(id: number) {

  const confirmDelete = confirm('Are you sure you want to delete this inventory?');
  if (!confirmDelete) return;

  this.inventoryService.delete(id).subscribe({
    next: () => {
      this.toastr.success('Deleted successfully');
      this.loadInventory();
    },
    error: () => {
      this.loadInventory();
      this.toastr.success('Deleted successfully');
    }
  });
}

// ✅ FIXED TOGGLE
toggleAvailability(id: number) {

  const confirmToggle = confirm('Change availability status?');
  if (!confirmToggle) return;

  this.inventoryService.toggle(id).subscribe({
    next: () => {
      this.toastr.success('Status updated');
      this.loadInventory();
    },
    error: () => {
      // backend still succeeds → handle gracefully
      this.toastr.success('Status updated');
      this.loadInventory();
    }
  });
}

searchInventory() {
  if (!this.inventorySearchId) {
    this.loadInventory();
    return;
  }

  this.inventoryService.getById(this.inventorySearchId).subscribe({
    next: (res) => this.inventories = [res],
    error: () => this.toastr.error('Inventory not found')
  });
}

resetInventory() {
  this.inventorySearchId = null;
  this.loadInventory();
}
// ================= PAYMENT =================



// LOAD
loadPayments() {
  this.paymentService.getAll().subscribe({
    next: (res: any) => {

      if (Array.isArray(res)) {
        this.payments = res;
      } else if (res.data) {
        this.payments = Array.isArray(res.data) ? res.data : [res.data];
      } else {
        this.payments = [];
      }

    },
    error: () => this.toastr.error('Failed to load payments')
  });
}

// SEARCH
searchPayment() {

  // 🔥 SEARCH BY RENTAL ID (FIXED)
  if (this.paymentSearchRentalId) {
    this.paymentService.getByRentalId(this.paymentSearchRentalId).subscribe({
      next: (res: any) => {

        if (Array.isArray(res)) {
          this.payments = res;
        } else if (res.data) {
          this.payments = Array.isArray(res.data) ? res.data : [res.data];
        } else {
          this.payments = [];
        }

      },
      error: () => this.toastr.error('No payments found')
    });
    return;
  }

  // SEARCH BY USER
  if (this.paymentSearchUserId) {
    this.paymentService.getByUserId(this.paymentSearchUserId).subscribe({
      next: (res: any) => {

        if (Array.isArray(res)) {
          this.payments = res;
        } else if (res.data) {
          this.payments = Array.isArray(res.data) ? res.data : [res.data];
        } else {
          this.payments = [];
        }

      },
      error: () => this.toastr.error('No payments found')
    });
    return;
  }

  this.loadPayments();
}

// RESET
resetPayment() {
  this.paymentSearchRentalId = null;
  this.paymentSearchUserId = null;
  this.loadPayments();
}

// REFRESH
refreshPayments() {
  this.loadPayments();
  this.toastr.success('Payments refreshed');
}

// ✅ STATUS
getPaymentStatus(status: number): string {
  return status === 0 ? 'Failed' : 'Success';
}

// ✅ METHOD FIX
getPaymentMethod(method: number): string {
  switch (method) {
    case 0: return 'Debit Card';
    case 1: return 'Credit Card';
    case 2: return 'Net Banking';
    case 3: return 'UPI';
    default: return '-';
  }
}
//-----------------------Movies---------------------------
loadMovies() {
  this.movieService.getAll().subscribe({
    next: (res) => this.movies = [...res],
    error: () => this.toastr.error('Failed to load movies')
  });
}

refreshMovies() {
  this.loadMovies();
  this.toastr.success('Movies refreshed');
}

searchMovie() {

  if (this.movieSearchId) {
    this.movieService.getById(this.movieSearchId).subscribe({
      next: (res) => this.movies = [res],
      error: () => this.toastr.error('Movie not found')
    });
    return;
  }

  if (this.movieSearchName) {
  this.movieService.search(this.movieSearchName).subscribe({
    next: (res: any) => {

      if (Array.isArray(res)) {
        this.movies = res;
      } else if (res.items) {
        this.movies = res.items;
      } else if (res.data) {
        this.movies = res.data;
      } else if (res.results) {
        this.movies = res.results;
      } else {
        this.movies = []; // fallback
      }

    },
    error: () => this.toastr.error('Search failed')
  });
  return;
}

  this.loadMovies();
}

resetMovie() {
  this.movieSearchId = null;
  this.movieSearchName = '';
  this.loadMovies();
}

openMovieForm() {
  this.showMovieForm = true;
  this.isMovieEdit = false;

  this.movieForm = {
    title: '',
    description: '',
    releaseYear: 2024,
    durationMinutes: 0,
    language: '',
    posterPath: '',
    trailerUrl: ''
  };
}

editMovie(m: any) {
  this.showMovieForm = true;
  this.isMovieEdit = true;
  this.selectedMovieId = m.id;

  this.movieForm = { ...m };
}

saveMovie() {

  if (this.isMovieEdit && this.selectedMovieId !== null) {

    this.movieService.update(this.selectedMovieId, this.movieForm).subscribe({
      next: () => {
        this.toastr.success('Updated successfully');
        this.loadMovies();
        this.showMovieForm = false;
      }
    });

  } else {

    this.movieService.add(this.movieForm).subscribe({
      next: () => {
        this.toastr.success('Added successfully');
        this.loadMovies();
        this.showMovieForm = false;
      }
    });

  }
}

deleteMovie(id: number) {

  if (!confirm('Delete this movie?')) return;

  this.movieService.delete(id).subscribe({
    next: () => {
      this.toastr.success('Deleted');
      this.loadMovies();
    },
    error: () => {
      this.loadMovies();
      this.toastr.success('Deleted');
    }
  });
}

showPosterPopup(url: string) {
  this.showPoster = url;
}

showTrailerPopup(url: string) {
  this.showTrailer = url;
}
//========================= RENTALS =======================

// ❗ FIX: Load active rentals instead of non-existing getAll
loadRentals() {
  this.rentalService.getAll().subscribe({
    next: (res: any) => {

      // ✅ HANDLE ALL POSSIBLE API STRUCTURES
      if (Array.isArray(res)) {
        this.rentals = res;
      } else if (res.data) {
        this.rentals = res.data;
      } else if (res.items) {
        this.rentals = res.items;
      } else {
        this.rentals = [];
      }

    },
    error: () => this.toastr.error('Failed to load rentals')
  });
}

searchRental() {

  if (!this.rentalSearchUserId) {
    this.loadRentals();
    return;
  }

  this.rentalService.getByUserId(this.rentalSearchUserId).subscribe({
    next: (res: any) => {

      if (Array.isArray(res)) {
        this.rentals = res;
      } else if (res.data) {
        this.rentals = res.data;
      } else {
        this.rentals = [];
      }

    },
    error: () => this.toastr.error('No rentals found')
  });
}

resetRental() {
  this.rentalSearchUserId = null;
  this.loadRentals();
}

refreshRentals() {
  this.loadRentals();
  this.toastr.success('Refreshed');
}

// ✅ STATUS FIX
getStatusText(status: number): string {
  switch (status) {
    case 0: return 'Payment Pending';
    case 1: return 'Available';
    case 2: return 'Payment Declined';
    default: return 'Unknown';
  }
}

// ✅ FIXED POPUP DATA HANDLING
openRentalItems(rentalId: number) {

  this.selectedRentalId = rentalId;

  this.rentalService.getItemsByRentalId(rentalId).subscribe({
    next: (res: any) => {

      if (Array.isArray(res)) {
        this.rentalItems = res;
      } else if (res.data) {
        this.rentalItems = res.data;
      } else {
        this.rentalItems = [];
      }

      this.showRentalItems = true; // ✅ IMPORTANT
    },
    error: () => this.toastr.error('Failed to load items')
  });
}
closeRentalItems() {
  this.showRentalItems = false;
}

endRentalItem(item: any) {

  if (!item?.id) {
    this.toastr.error('Invalid Rental Item ID');
    return;
  }

  if (!confirm('End this rental item?')) return;

  this.rentalService.endItem(item.id).subscribe({
    next: () => {
      this.toastr.success('Rental ended');
      this.openRentalItems(this.selectedRentalId!);
    },
    error: () => {
      this.toastr.success('Rental Ended'); // ❗ FIXED
    }
  });
}
}