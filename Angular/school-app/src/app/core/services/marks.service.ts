import { Injectable, inject } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import {
  CreateMarkRequest,
  MarkResponse,
  ReportCard,
  MarksChartData
} from '../models/marks.models';

@Injectable({ providedIn: 'root' })
export class MarksService {
  private http    = inject(HttpClient);
  private baseUrl = `${environment.apiUrl}/marks`;

  createBulkMarks(
    marks: CreateMarkRequest[]
  ): Observable<{ message: string }> {
    return this.http.post<{ message: string }>(this.baseUrl, marks);
  }

  getStudentMarks(studentId: string): Observable<MarkResponse[]> {
    return this.http.get<MarkResponse[]>(
      `${this.baseUrl}/student/${studentId}`
    );
  }

  getReportCard(studentId: string): Observable<ReportCard> {
    return this.http.get<ReportCard>(
      `${this.baseUrl}/report-card/${studentId}`
    );
  }

  getClassMarks(classId: string, examType: string): Observable<MarkResponse[]> {
    const params = new HttpParams().set('examType', examType);
    return this.http.get<MarkResponse[]>(
      `${this.baseUrl}/class/${classId}`, { params }
    );
  }

  getChartData(studentId: string): Observable<MarksChartData> {
    return this.http.get<MarksChartData>(
      `${this.baseUrl}/chart-data/${studentId}`
    );
  }

  updateMark(
    markId: string,
    marksObtained: number
  ): Observable<MarkResponse> {
    return this.http.put<MarkResponse>(
      `${this.baseUrl}/${markId}`,
      { marksObtained }
    );
  }

  getMyReportCard(): Observable<ReportCard> {
  return this.http.get<ReportCard>(`${this.baseUrl}/my-report-card`);
}
}