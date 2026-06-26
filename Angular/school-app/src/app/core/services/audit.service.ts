import { Injectable, inject } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import { AuditLogResponse } from '../models/audit.models';

@Injectable({ providedIn: 'root' })
export class AuditService {
  private http    = inject(HttpClient);
  private baseUrl = `${environment.apiUrl}/audit`;

  getAuditLogs(
    page      = 1,
    pageSize  = 50,
    entity?:  string,
    action?:  string
  ): Observable<AuditLogResponse> {
    let params = new HttpParams()
      .set('page', page)
      .set('pageSize', pageSize);

    if (entity) params = params.set('entity', entity);
    if (action) params = params.set('action', action);

    return this.http.get<AuditLogResponse>(this.baseUrl, { params });
  }
}