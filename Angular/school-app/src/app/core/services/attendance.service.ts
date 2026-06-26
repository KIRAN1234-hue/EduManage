import { Injectable, inject } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import {
  MarkAttendanceRequest,
  AttendanceResponse,
  AttendancePercentage
} from '../models/attendance.models';

@Injectable({ providedIn: 'root' })
export class AttendanceService {
  private http    = inject(HttpClient);
  private baseUrl = `${environment.apiUrl}/attendance`;

  // Teacher marks bulk attendance
  markBulkAttendance(
    records: MarkAttendanceRequest[]
  ): Observable<{ message: string }> {
    return this.http.post<{ message: string }>(this.baseUrl, records);
  }

  // Get student's full attendance record
  getStudentAttendance(studentId: string): Observable<AttendanceResponse[]> {
    return this.http.get<AttendanceResponse[]>(
      `${this.baseUrl}/student/${studentId}`
    );
  }

  // Get subject-wise percentage — used for student/parent dashboards
  getAttendancePercentage(
    studentId: string
  ): Observable<AttendancePercentage[]> {
    return this.http.get<AttendancePercentage[]>(
      `${this.baseUrl}/percentage/${studentId}`
    );
  }

  // Get class attendance for a specific date
  getClassAttendanceForDate(
    classId: string,
    date: string
  ): Observable<AttendanceResponse[]> {
    const params = new HttpParams().set('date', date);
    return this.http.get<AttendanceResponse[]>(
      `${this.baseUrl}/class/${classId}`, { params }
    );
  }

  // Update a past attendance record
  updateAttendance(
    attendanceId: string,
    status: string,
    remarks?: string
  ): Observable<AttendanceResponse> {
    return this.http.put<AttendanceResponse>(
      `${this.baseUrl}/${attendanceId}`,
      { status, remarks }
    );
  }

  getMyAttendance(): Observable<AttendanceResponse[]> {
  return this.http.get<AttendanceResponse[]>(`${this.baseUrl}/my`);
}

getMyAttendancePercentage(): Observable<AttendancePercentage[]> {
  return this.http.get<AttendancePercentage[]>(
    `${this.baseUrl}/my-percentage`
  );
}
}