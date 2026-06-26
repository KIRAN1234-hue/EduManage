import { Injectable, inject } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import { TimetableResponse, CreateTimetableRequest } from '../models/timetable.models';

@Injectable({ providedIn: 'root' })
export class TimetableService {
  private http    = inject(HttpClient);
  private baseUrl = `${environment.apiUrl}/timetable`;

  createSlot(dto: CreateTimetableRequest): Observable<TimetableResponse> {
    return this.http.post<TimetableResponse>(this.baseUrl, dto);
  }

  getClassTimetable(classId: string, academicYear = '2024-25'): Observable<TimetableResponse[]> {
    const params = new HttpParams().set('academicYear', academicYear);
    return this.http.get<TimetableResponse[]>(
      `${this.baseUrl}/class/${classId}`, { params }
    );
  }

  getMyTimetable(academicYear = '2024-25'): Observable<TimetableResponse[]> {
    const params = new HttpParams().set('academicYear', academicYear);
    return this.http.get<TimetableResponse[]>(`${this.baseUrl}/my`, { params });
  }

  deleteSlot(id: string): Observable<{ message: string }> {
    return this.http.delete<{ message: string }>(`${this.baseUrl}/${id}`);
  }

  getMyClassTimetable(academicYear = '2024-25'): Observable<TimetableResponse[]> {
  const params = new HttpParams().set('academicYear', academicYear);
  return this.http.get<TimetableResponse[]>(
    `${this.baseUrl}/my-class`, { params }
  );
}
}