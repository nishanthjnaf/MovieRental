import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';

@Injectable({
  providedIn: 'root'
})
export class RentalService {

  private baseUrl = 'http://localhost:5287/api/Rental';

  constructor(private http: HttpClient) {}

  // ===================== GET ALL RENTALS =====================
  getAll(): Observable<any[]> {
    return this.http.get<any[]>(this.baseUrl);
  }

  // ===================== GET RENTAL ITEMS =====================
  getItemsByRentalId(rentalId: number): Observable<any[]> {
    return this.http.get<any[]>(`${this.baseUrl}/${rentalId}/items`);
  }

  // ===================== GET RENTALS BY USER =====================
  getByUserId(userId: number): Observable<any[]> {
    return this.http.get<any[]>(`${this.baseUrl}/user/${userId}`);
  }

  // ===================== GET ACTIVE RENTALS =====================
  getActiveByUserId(userId: number): Observable<any[]> {
    return this.http.get<any[]>(`${this.baseUrl}/active/${userId}`);
  }

  // ===================== END RENTAL ITEM =====================
  endItem(rentalItemId: number): Observable<any> {
    return this.http.patch(
      `${this.baseUrl}/end-item/${rentalItemId}`,
      {},
      { responseType: 'text' }
    );
  }

  // ===================== RENEW RENTAL ITEM =====================
  renewItem(rentalItemId: number, daysToAdd: number): Observable<any> {
    return this.http.patch(
      `${this.baseUrl}/renew-item/${rentalItemId}`,
      { daysToAdd }
    );
  }

  // ===================== CREATE RENTAL =====================
  createRental(data: any): Observable<any> {
    return this.http.post(this.baseUrl, data);
  }

}