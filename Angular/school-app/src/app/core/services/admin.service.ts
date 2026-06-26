import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import {
  CreateClassRequest, ClassResponse,
  CreateSubjectRequest, SubjectResponse,
  CreateTeacherRequest, TeacherResponse,
  CreateStudentRequest, StudentResponse,
  RegisterTeacherRequest, RegisterStudentRequest,
  RegisterParentRequest,TeacherForDoubt
} from '../models/admin.models';

@Injectable({ providedIn: 'root' })
export class AdminService {
  private http    = inject(HttpClient);
  private baseUrl = `${environment.apiUrl}/admin`;
  private authUrl = `${environment.apiUrl}/auth`;

  // ── Classes ───────────────────────────────────────────────────────────────
  createClass(dto: CreateClassRequest): Observable<ClassResponse> {
    return this.http.post<ClassResponse>(`${this.baseUrl}/classes`, dto);
  }

  getAllClasses(): Observable<ClassResponse[]> {
    return this.http.get<ClassResponse[]>(`${this.baseUrl}/classes`);
  }

  // ── Subjects ──────────────────────────────────────────────────────────────
  createSubject(dto: CreateSubjectRequest): Observable<SubjectResponse> {
    return this.http.post<SubjectResponse>(`${this.baseUrl}/subjects`, dto);
  }

  getAllSubjects(): Observable<SubjectResponse[]> {
    return this.http.get<SubjectResponse[]>(`${this.baseUrl}/subjects`);
  }

  getSubjectsByClass(classId: string): Observable<SubjectResponse[]> {
    return this.http.get<SubjectResponse[]>(
      `${this.baseUrl}/subjects/class/${classId}`
    );
  }

  // ── Teachers ──────────────────────────────────────────────────────────────
  createTeacher(dto: CreateTeacherRequest): Observable<TeacherResponse> {
    return this.http.post<TeacherResponse>(`${this.baseUrl}/teachers`, dto);
  }

  getAllTeachers(): Observable<TeacherResponse[]> {
    return this.http.get<TeacherResponse[]>(`${this.baseUrl}/teachers`);
  }

  getTeachersForDoubts(): Observable<TeacherForDoubt[]> {
  return this.http.get<TeacherForDoubt[]>(
    `${this.baseUrl}/teachers-for-doubts`
  );
}

  // ── Students ──────────────────────────────────────────────────────────────
  createStudent(dto: CreateStudentRequest): Observable<StudentResponse> {
    return this.http.post<StudentResponse>(`${this.baseUrl}/students`, dto);
  }

  getAllStudents(): Observable<StudentResponse[]> {
    return this.http.get<StudentResponse[]>(`${this.baseUrl}/students`);
  }

  getStudentsByClass(classId: string): Observable<StudentResponse[]> {
    return this.http.get<StudentResponse[]>(
      `${this.baseUrl}/students/class/${classId}`
    );
  }

  // ── Self Registrations ────────────────────────────────────────────────────
  registerTeacher(dto: RegisterTeacherRequest): Observable<{ message: string }> {
    return this.http.post<{ message: string }>(
      `${this.authUrl}/register/teacher`, dto
    );
  }

  registerStudent(dto: RegisterStudentRequest): Observable<{ message: string }> {
    return this.http.post<{ message: string }>(
      `${this.authUrl}/register/student`, dto
    );
  }

  registerParent(dto: RegisterParentRequest): Observable<{ message: string }> {
    return this.http.post<{ message: string }>(
      `${this.authUrl}/register/parent`, dto
    );
  }
  // Add these 4 methods:
deleteClass(classId: string): Observable<{ message: string }> {
  return this.http.delete<{ message: string }>(
    `${this.baseUrl}/classes/${classId}`
  );
}

deleteSubject(subjectId: string): Observable<{ message: string }> {
  return this.http.delete<{ message: string }>(
    `${this.baseUrl}/subjects/${subjectId}`
  );
}

deleteTeacher(teacherId: string): Observable<{ message: string }> {
  return this.http.delete<{ message: string }>(
    `${this.baseUrl}/teachers/${teacherId}`
  );
}

deleteStudent(studentId: string): Observable<{ message: string }> {
  return this.http.delete<{ message: string }>(
    `${this.baseUrl}/students/${studentId}`
  );
}
}