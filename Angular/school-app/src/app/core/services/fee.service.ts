import { Injectable, inject } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import {
  FeeStructureResponse, CreateFeeStructureRequest,
  RecordPaymentRequest, FeePaymentResponse, StudentFeeStatus,
  ParentPaymentRequest
} from '../models/fee.models';

@Injectable({ providedIn: 'root' })
export class FeeService {
  private http    = inject(HttpClient);
  private baseUrl = `${environment.apiUrl}/fees`;

  createFeeStructure(dto: CreateFeeStructureRequest): Observable<FeeStructureResponse> {
    return this.http.post<FeeStructureResponse>(`${this.baseUrl}/structures`, dto);
  }

  getFeeStructures(academicYear = '2024-25'): Observable<FeeStructureResponse[]> {
    return this.http.get<FeeStructureResponse[]>(`${this.baseUrl}/structures`,
      { params: new HttpParams().set('academicYear', academicYear) });
  }

  getClassFeeStructures(classId: string, academicYear = '2024-25'): Observable<FeeStructureResponse[]> {
    return this.http.get<FeeStructureResponse[]>(
      `${this.baseUrl}/structures/class/${classId}`,
      { params: new HttpParams().set('academicYear', academicYear) }
    );
  }

  recordPayment(dto: RecordPaymentRequest): Observable<FeePaymentResponse> {
    return this.http.post<FeePaymentResponse>(`${this.baseUrl}/payments`, dto);
  }

  getPaymentsByStructure(structureId: string): Observable<FeePaymentResponse[]> {
    return this.http.get<FeePaymentResponse[]>(
      `${this.baseUrl}/payments/structure/${structureId}`
    );
  }

  getStudentFeeStatus(studentId: string, academicYear = '2024-25'): Observable<StudentFeeStatus> {
    return this.http.get<StudentFeeStatus>(
      `${this.baseUrl}/student/${studentId}/status`,
      { params: new HttpParams().set('academicYear', academicYear) }
    );
  }

  getMyFeeStatus(academicYear = '2024-25'): Observable<StudentFeeStatus> {
    return this.http.get<StudentFeeStatus>(`${this.baseUrl}/my-status`,
      { params: new HttpParams().set('academicYear', academicYear) });
  }

  getMyChildFeeStatus(academicYear = '2024-25'): Observable<StudentFeeStatus> {
  return this.http.get<StudentFeeStatus>(`${this.baseUrl}/my-child-status`,
    { params: new HttpParams().set('academicYear', academicYear) });
}

parentPayment(dto: ParentPaymentRequest): Observable<FeePaymentResponse> {
  return this.http.post<FeePaymentResponse>(
    `${this.baseUrl}/parent-payment`, dto
  );
}
getStudentBalance(studentId: string, feeStructureId: string): Observable<{
  totalAmount: number;
  paid: number;
  remaining: number;
  isFullyPaid: boolean;
}> {
  const params = new HttpParams()
    .set('studentId', studentId)
    .set('feeStructureId', feeStructureId);
  return this.http.get<any>(`${this.baseUrl}/balance`, { params });
}
}