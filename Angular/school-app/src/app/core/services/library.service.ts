import { Injectable, inject } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import {
  LibraryBookResponse, CreateBookRequest,
  BookIssueResponse, IssueBookRequest
} from '../models/library.models';

@Injectable({ providedIn: 'root' })
export class LibraryService {
  private http    = inject(HttpClient);
  private baseUrl = `${environment.apiUrl}/library`;

  addBook(dto: CreateBookRequest): Observable<LibraryBookResponse> {
    return this.http.post<LibraryBookResponse>(`${this.baseUrl}/books`, dto);
  }

  getAllBooks(search?: string, category?: string): Observable<LibraryBookResponse[]> {
    let params = new HttpParams();
    if (search)   params = params.set('search', search);
    if (category) params = params.set('category', category);
    return this.http.get<LibraryBookResponse[]>(`${this.baseUrl}/books`, { params });
  }

  issueBook(dto: IssueBookRequest): Observable<BookIssueResponse> {
    return this.http.post<BookIssueResponse>(`${this.baseUrl}/issue`, dto);
  }

  returnBook(issueId: string): Observable<BookIssueResponse> {
    return this.http.put<BookIssueResponse>(`${this.baseUrl}/return/${issueId}`, {});
  }

  getActiveIssues(): Observable<BookIssueResponse[]> {
    return this.http.get<BookIssueResponse[]>(`${this.baseUrl}/issues/active`);
  }

  getOverdueIssues(): Observable<BookIssueResponse[]> {
    return this.http.get<BookIssueResponse[]>(`${this.baseUrl}/issues/overdue`);
  }

  getStudentIssues(studentId: string): Observable<BookIssueResponse[]> {
    return this.http.get<BookIssueResponse[]>(
      `${this.baseUrl}/issues/student/${studentId}`
    );
  }

  getMyIssues(): Observable<BookIssueResponse[]> {
    return this.http.get<BookIssueResponse[]>(`${this.baseUrl}/my-issues`);
  }

  deleteBook(bookId: string): Observable<{ message: string }> {
    return this.http.delete<{ message: string }>(`${this.baseUrl}/books/${bookId}`);
  }
}   