import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import { DashboardStats } from '../models/dashboard.models';

@Injectable({ providedIn: 'root' })
export class DashboardService {
  private http    = inject(HttpClient);
  private baseUrl = `${environment.apiUrl}/dashboard`;

  getPrincipalStats(): Observable<DashboardStats> {
    return this.http.get<DashboardStats>(`${this.baseUrl}/principal`);
  }

  getTeacherStats(): Observable<DashboardStats> {
    return this.http.get<DashboardStats>(`${this.baseUrl}/teacher`);
  }

  getStudentStats(): Observable<DashboardStats> {
    return this.http.get<DashboardStats>(`${this.baseUrl}/student`);
  }
}