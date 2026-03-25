import { ChangeDetectorRef, Component, EventEmitter, Input, OnInit, Output } from '@angular/core';
import { CommonModule } from '@angular/common';
import { UserService } from '../services/user';
import { GenreService } from '../services/genre';
import { ThemeService } from '../services/theme';
import { catchError, of } from 'rxjs';

const LANGUAGES = ['English', 'Hindi', 'Tamil', 'Telugu', 'Malayalam', 'Kannada', 'Bengali', 'Marathi', 'French', 'Spanish', 'Korean', 'Japanese'];

@Component({
  selector: 'app-preferences-setup',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './preferences-setup.html'
})
export class PreferencesSetup implements OnInit {
  @Input() userId = 0;
  @Input() editMode = false; // true = editing existing prefs, false = first-time setup
  @Output() done = new EventEmitter<void>();
  @Output() cancelled = new EventEmitter<void>();

  step = 1;
  genres: any[] = [];
  languages = LANGUAGES;

  selectedGenres: Set<string> = new Set();
  selectedLanguages: Set<string> = new Set();
  selectedTheme = 'dark';
  saving = false;

  constructor(
    private userService: UserService,
    private genreService: GenreService,
    private themeService: ThemeService,
    private cdr: ChangeDetectorRef
  ) {}

  ngOnInit() {
    this.genreService.getAllGenres().pipe(catchError(() => of([]))).subscribe(res => {
      this.genres = res || [];
      this.cdr.detectChanges();
    });

    this.selectedTheme = this.themeService.isDark ? 'dark' : 'light';

    // If editing, pre-load existing preferences
    if (this.editMode && this.userId > 0) {
      this.userService.getPreferences(this.userId).pipe(catchError(() => of(null))).subscribe(pref => {
        if (pref) {
          this.selectedGenres = new Set(pref.preferredGenres || []);
          this.selectedLanguages = new Set(pref.preferredLanguages || []);
          this.selectedTheme = pref.theme || 'dark';
        }
        this.cdr.detectChanges();
      });
    }
  }

  toggleGenre(name: string) {
    this.selectedGenres.has(name) ? this.selectedGenres.delete(name) : this.selectedGenres.add(name);
  }

  toggleLanguage(name: string) {
    this.selectedLanguages.has(name) ? this.selectedLanguages.delete(name) : this.selectedLanguages.add(name);
  }

  setTheme(t: string) {
    this.selectedTheme = t;
    if (t === 'dark') {
      document.documentElement.classList.add('dark-theme');
      document.documentElement.classList.remove('light-theme');
    } else {
      document.documentElement.classList.add('light-theme');
      document.documentElement.classList.remove('dark-theme');
    }
  }

  next() { this.step++; }
  back() { this.step--; }

  save() {
    this.saving = true;
    this.userService.savePreferences(this.userId, {
      preferredGenres: Array.from(this.selectedGenres),
      preferredLanguages: Array.from(this.selectedLanguages),
      theme: this.selectedTheme
    }).pipe(catchError(() => of(null))).subscribe(() => {
      localStorage.setItem('cinefilia_theme', this.selectedTheme);
      this.saving = false;
      this.done.emit();
    });
  }

  skip() {
    this.userService.savePreferences(this.userId, {
      preferredGenres: [],
      preferredLanguages: [],
      theme: this.selectedTheme
    }).pipe(catchError(() => of(null))).subscribe(() => {
      this.done.emit();
    });
  }

  cancel() {
    this.cancelled.emit();
  }
}
