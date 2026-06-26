import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import { NoticeResponse, CreateNoticeRequest } from '../models/notice.models';

@Injectable({ providedIn: 'root' })
export class NoticeService {
  private http    = inject(HttpClient);
  private baseUrl = `${environment.apiUrl}/notices`;

  createNotice(dto: CreateNoticeRequest): Observable<NoticeResponse> {
    return this.http.post<NoticeResponse>(this.baseUrl, dto);
  }

  getNotices(): Observable<NoticeResponse[]> {
    return this.http.get<NoticeResponse[]>(this.baseUrl);
  }

  acknowledgeNotice(noticeId: string): Observable<{ message: string }> {
    return this.http.post<{ message: string }>(
      `${this.baseUrl}/${noticeId}/acknowledge`, {}
    );
  }

  archiveNotice(noticeId: string): Observable<{ message: string }> {
    return this.http.put<{ message: string }>(
      `${this.baseUrl}/${noticeId}/archive`, {}
    );
  }

  deleteNotice(noticeId: string): Observable<{ message: string }> {
    return this.http.delete<{ message: string }>(
      `${this.baseUrl}/${noticeId}`
    );
  }
}