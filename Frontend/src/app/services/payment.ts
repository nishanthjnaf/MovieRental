import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';

@Injectable({ providedIn: 'root' })
export class PaymentService {

  baseUrl = 'http://localhost:5287/api/Payment';

  constructor(private http: HttpClient) {}

  getAll() {
    return this.http.get<any[]>(this.baseUrl);
  }

  getByRentalId(rentalId: number) {
    return this.http.get<any>(`${this.baseUrl}/rental/${rentalId}`);
  }

  getByUserId(userId: number) {
    return this.http.get<any[]>(`${this.baseUrl}/user/${userId}`);
  }

  makePayment(data: any) {
    return this.http.post<any>(this.baseUrl, data);
  }

  processRefund(rentalItemId: number) {
    return this.http.post<any>(`${this.baseUrl}/refund/${rentalItemId}`, {});
  }

  getItemRefund(rentalItemId: number) {
    return this.http.get<any>(`${this.baseUrl}/item-refund/${rentalItemId}`);
  }
}