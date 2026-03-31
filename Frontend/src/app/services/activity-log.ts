import { Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';

@Injectable({ providedIn: 'root' })
export class ActivityLogService {
  private baseUrl = 'http://localhost:5287/api/ActivityLog';

  constructor(private http: HttpClient) {}

  getLogs(filters: {
    userId?: number;
    role?: string;
    entity?: string;
    action?: string;
    status?: string;
    from?: string;
    to?: string;
    sortOrder?: string;
    page?: number;
    pageSize?: number;
  }) {
    let params = new HttpParams();
    if (filters.userId) params = params.set('userId', filters.userId);
    if (filters.role) params = params.set('role', filters.role);
    if (filters.entity) params = params.set('entity', filters.entity);
    if (filters.action) params = params.set('action', filters.action);
    if (filters.status) params = params.set('status', filters.status);
    if (filters.from) params = params.set('from', filters.from);
    if (filters.to) params = params.set('to', filters.to);
    if (filters.sortOrder) params = params.set('sortOrder', filters.sortOrder);
    if (filters.page) params = params.set('page', filters.page);
    if (filters.pageSize) params = params.set('pageSize', filters.pageSize);
    return this.http.get<any>(this.baseUrl, { params });
  }
}
