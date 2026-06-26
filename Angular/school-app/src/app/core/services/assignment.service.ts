import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import {
  AssignmentResponse, CreateAssignmentRequest,
  SubmitAssignmentRequest, SubmissionResponse,
  GradeSubmissionRequest
} from '../models/assignment.models';

@Injectable({ providedIn: 'root' })
export class AssignmentService {
  private http    = inject(HttpClient);
  private baseUrl = `${environment.apiUrl}/assignments`;

  createAssignment(dto: CreateAssignmentRequest): Observable<AssignmentResponse> {
    return this.http.post<AssignmentResponse>(this.baseUrl, dto);
  }

  getClassAssignments(classId: string): Observable<AssignmentResponse[]> {
    return this.http.get<AssignmentResponse[]>(
      `${this.baseUrl}/class/${classId}`
    );
  }

  getMyAssignments(): Observable<AssignmentResponse[]> {
    return this.http.get<AssignmentResponse[]>(
      `${this.baseUrl}/my-assignments`
    );
  }

  getAssignment(id: string): Observable<AssignmentResponse> {
    return this.http.get<AssignmentResponse>(`${this.baseUrl}/${id}`);
  }

  submitAssignment(
    assignmentId: string,
    dto: SubmitAssignmentRequest
  ): Observable<SubmissionResponse> {
    return this.http.post<SubmissionResponse>(
      `${this.baseUrl}/${assignmentId}/submit`, dto
    );
  }

  getSubmissions(assignmentId: string): Observable<SubmissionResponse[]> {
    return this.http.get<SubmissionResponse[]>(
      `${this.baseUrl}/${assignmentId}/submissions`
    );
  }

  gradeSubmission(
    submissionId: string,
    dto: GradeSubmissionRequest
  ): Observable<SubmissionResponse> {
    return this.http.put<SubmissionResponse>(
      `${this.baseUrl}/submissions/${submissionId}/grade`, dto
    );
  }

  getMySubmission(assignmentId: string): Observable<SubmissionResponse | null> {
    return this.http.get<SubmissionResponse | null>(
      `${this.baseUrl}/${assignmentId}/my-submission`
    );
  }

  getMyClassAssignments(): Observable<AssignmentResponse[]> {
  return this.http.get<AssignmentResponse[]>(
    `${this.baseUrl}/my-class`
  );
}
}