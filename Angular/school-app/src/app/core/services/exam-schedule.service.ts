import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import { ExamScheduleResponse, CreateExamScheduleRequest } from '../models/exam-schedule.models';

@Injectable({ providedIn: 'root' })
export class ExamScheduleService {
  private http    = inject(HttpClient);
  private baseUrl = `${environment.apiUrl}/exam-schedule`;

  createExam(dto: CreateExamScheduleRequest): Observable<ExamScheduleResponse> {
    return this.http.post<ExamScheduleResponse>(this.baseUrl, dto);
  }

  getClassExams(classId: string): Observable<ExamScheduleResponse[]> {
    return this.http.get<ExamScheduleResponse[]>(
      `${this.baseUrl}/class/${classId}`
    );
  }

  getUpcomingExams(): Observable<ExamScheduleResponse[]> {
    return this.http.get<ExamScheduleResponse[]>(`${this.baseUrl}/upcoming`);
  }

  toggleMarksEntry(id: string): Observable<{ message: string }> {
    return this.http.put<{ message: string }>(
      `${this.baseUrl}/${id}/toggle-marks-entry`, {}
    );
  }

  deleteExam(id: string): Observable<{ message: string }> {
    return this.http.delete<{ message: string }>(`${this.baseUrl}/${id}`);
  }
  getMyClassExams(): Observable<ExamScheduleResponse[]> {
  return this.http.get<ExamScheduleResponse[]>(`${this.baseUrl}/my-class`);
}
}