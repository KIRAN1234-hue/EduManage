import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import {
  ComplaintResponse, CreateComplaintRequest, UpdateComplaintRequest
} from '../models/complaint.models';

@Injectable({ providedIn: 'root' })
export class ComplaintService {
  private http    = inject(HttpClient);
  private baseUrl = `${environment.apiUrl}/complaints`;

  submitComplaint(dto: CreateComplaintRequest): Observable<ComplaintResponse> {
    return this.http.post<ComplaintResponse>(this.baseUrl, dto);
  }

  getMyComplaints(): Observable<ComplaintResponse[]> {
    return this.http.get<ComplaintResponse[]>(`${this.baseUrl}/mine`);
  }

  getAllComplaints(status?: string): Observable<ComplaintResponse[]> {
    const url = status
      ? `${this.baseUrl}/all?status=${status}`
      : `${this.baseUrl}/all`;
    return this.http.get<ComplaintResponse[]>(url);
  }

  updateComplaint(
    id: string,
    dto: UpdateComplaintRequest
  ): Observable<ComplaintResponse> {
    return this.http.put<ComplaintResponse>(
      `${this.baseUrl}/${id}/update`, dto
    );
  }
}