import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';

@Injectable({ providedIn: 'root' })
export class GenreService {

  private baseUrl = 'http://localhost:5287/api/Genre';

  constructor(private http: HttpClient) {}

  // GET ALL
  getAllGenres() {
    return this.http.get<any[]>(this.baseUrl);
  }

  // GET BY ID
  getGenreById(id: number) {
    return this.http.get<any>(`${this.baseUrl}/${id}`);
  }

  // ADD
  addGenre(data: any) {
    return this.http.post(this.baseUrl, data);
  }

  // UPDATE
  updateGenre(id: number, data: any) {
    return this.http.put(`${this.baseUrl}/${id}`, data);
  }

  // DELETE
  deleteGenre(id: number) {
    return this.http.delete(`${this.baseUrl}/${id}`);
  }

  // ASSIGN MOVIE
  assignMovie(genreId: number, movieId: number) {
    return this.http.post(`${this.baseUrl}/${genreId}/assign/${movieId}`, {});
  }
}