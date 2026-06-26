import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import {
  LeaveResponse, CreateLeaveRequest,
  ApproveLeaveRequest
} from '../models/leave.models';

@Injectable({ providedIn: 'root' })
export class LeaveService {
  private http    = inject(HttpClient);
  private baseUrl = `${environment.apiUrl}/leave`;

  applyLeave(dto: CreateLeaveRequest): Observable<LeaveResponse> {
    return this.http.post<LeaveResponse>(this.baseUrl, dto);
  }

  getMyLeaves(): Observable<LeaveResponse[]> {
    return this.http.get<LeaveResponse[]>(`${this.baseUrl}/my`);
  }

  getPendingLeaves(): Observable<LeaveResponse[]> {
    return this.http.get<LeaveResponse[]>(`${this.baseUrl}/pending`);
  }

  processLeave(
    leaveId: string,
    dto: ApproveLeaveRequest
  ): Observable<LeaveResponse> {
    return this.http.put<LeaveResponse>(
      `${this.baseUrl}/${leaveId}/process`, dto
    );
  }
}