import {
  Component, OnInit, inject, signal
} from '@angular/core';
import { CommonModule } from '@angular/common';
import {
  ReactiveFormsModule, FormBuilder,
  FormGroup, FormsModule
} from '@angular/forms';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatSelectModule } from '@angular/material/select';
import { MatDatepickerModule } from '@angular/material/datepicker';
import { MatNativeDateModule } from '@angular/material/core';
import { MatInputModule } from '@angular/material/input';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatSnackBar, MatSnackBarModule } from '@angular/material/snack-bar';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatTooltipModule } from '@angular/material/tooltip';
import { MatChipsModule } from '@angular/material/chips';

import { AttendanceService } from '../../../core/services/attendance.service';
import { AdminService } from '../../../core/services/admin.service';
import {
  StudentAttendanceRow,
  MarkAttendanceRequest
} from '../../../core/models/attendance.models';
import {
  ClassResponse, SubjectResponse
} from '../../../core/models/admin.models';

type AttendanceStatus = 'Present' | 'Absent' | 'Late' | 'OnLeave';

@Component({
  selector: 'app-teacher-attendance',
  standalone: true,
  imports: [
    CommonModule, ReactiveFormsModule, FormsModule,
    MatFormFieldModule, MatSelectModule, MatDatepickerModule,
    MatNativeDateModule, MatInputModule, MatButtonModule,
    MatIconModule, MatSnackBarModule, MatProgressSpinnerModule,
    MatTooltipModule, MatChipsModule
  ],
  templateUrl: './teacher-attendance.component.html',
  styleUrls: ['./teacher-attendance.component.scss']
})
export class TeacherAttendanceComponent implements OnInit {
  private attendanceService = inject(AttendanceService);
  private adminService      = inject(AdminService);
  private snackBar          = inject(MatSnackBar);
  private fb                = inject(FormBuilder);

  filterForm!: FormGroup;
  isLoading     = signal(false);
  isSubmitting  = signal(false);
  studentsLoaded = signal(false);

  selectedDate = new Date();
  maxDate      = new Date();

  students: StudentAttendanceRow[] = [];

  // Loaded from API
  classes:  ClassResponse[]   = [];
  subjects: SubjectResponse[]  = [];

  statusOptions: { value: AttendanceStatus; label: string; color: string }[] = [
    { value: 'Present', label: 'P',  color: '#16A34A' },
    { value: 'Absent',  label: 'A',  color: '#DC2626' },
    { value: 'Late',    label: 'L',  color: '#D97706' },
    { value: 'OnLeave', label: 'OL', color: '#64748B' },
  ];

  ngOnInit(): void {
    this.filterForm = this.fb.group({
      classId:   [''],
      subjectId: ['']
    });

    // Load classes from real API
    this.adminService.getAllClasses().subscribe({
      next: data => this.classes = data,
      error: ()  => console.error('Failed to load classes')
    });
  }

  onClassChange(): void {
    const classId = this.filterForm.value.classId;
    this.subjects = [];
    this.filterForm.patchValue({ subjectId: '' });
    this.studentsLoaded.set(false);
    this.students = [];

    if (!classId) return;

    // Load subjects for selected class from real API
    this.adminService.getSubjectsByClass(classId).subscribe({
      next: data => this.subjects = data,
      error: ()  => console.error('Failed to load subjects')
    });
  }

  loadStudents(): void {
    const { classId, subjectId } = this.filterForm.value;
    if (!classId || !subjectId) return;

    this.isLoading.set(true);
    this.studentsLoaded.set(false);
    this.students = [];

    // Load REAL students from API by classId
    this.adminService.getStudentsByClass(classId).subscribe({
      next: (data) => {
        // Map StudentResponse to StudentAttendanceRow
        this.students = data.map(s => ({
          studentId:   s.id,
          studentName: s.fullName,
          rollNumber:  s.rollNumber,
          status:      'Present' as AttendanceStatus,
          remarks:     ''
        }));

        if (this.students.length === 0) {
          this.snackBar.open(
            'No students found in this class. Enroll students first.',
            'Close', { duration: 4000 }
          );
        }

        this.isLoading.set(false);
        this.studentsLoaded.set(true);
      },
      error: () => {
        this.snackBar.open('Failed to load students.', 'Close', { duration: 3000 });
        this.isLoading.set(false);
      }
    });
  }

  markAllPresent(): void {
    this.students = this.students.map(s => ({ ...s, status: 'Present' as AttendanceStatus }));
  }

  markAllAbsent(): void {
    this.students = this.students.map(s => ({ ...s, status: 'Absent' as AttendanceStatus }));
  }

  setStatus(studentId: string, status: AttendanceStatus): void {
    this.students = this.students.map(s =>
      s.studentId === studentId ? { ...s, status } : s
    );
  }

  getStatusCount(status: AttendanceStatus): number {
    return this.students.filter(s => s.status === status).length;
  }

  getStatusColor(status: string): string {
    const map: Record<string, string> = {
      Present: '#16A34A', Absent: '#DC2626',
      Late: '#D97706', OnLeave: '#64748B'
    };
    return map[status] ?? '#64748B';
  }

  getStatusBg(status: string): string {
    const map: Record<string, string> = {
      Present: '#DCFCE7', Absent: '#FEE2E2',
      Late: '#FEF3C7', OnLeave: '#F1F5F9'
    };
    return map[status] ?? '#F1F5F9';
  }

  formatDate(date: Date): string {
  const year  = date.getFullYear();
  const month = String(date.getMonth() + 1).padStart(2, '0');
  const day   = String(date.getDate()).padStart(2, '0');

  return `${year}-${month}-${day}`;
}

onDateChange(event: any): void {
  this.selectedDate = event.value;
  this.loadStudents();
}

  submitAttendance(): void {
    if (!this.studentsLoaded()) return;

    const { subjectId } = this.filterForm.value;
    const dateStr = this.formatDate(this.selectedDate);

    const records: MarkAttendanceRequest[] = this.students.map(s => ({
      studentId: s.studentId,
      subjectId,
      date:      dateStr,
      status:    s.status,
      remarks:   s.remarks || undefined
    }));

    this.isSubmitting.set(true);

    this.attendanceService.markBulkAttendance(records).subscribe({
      next: (res) => {
        this.snackBar.open(res.message, 'Close', {
          duration: 4000,
          panelClass: ['success-snackbar'],
          horizontalPosition: 'center',
          verticalPosition: 'top'
        });
        this.isSubmitting.set(false);
      },
      error: (err) => {
        const msg = err.error?.message ?? 'Failed to submit attendance.';
        this.snackBar.open(msg, 'Close', {
          duration: 4000,
          panelClass: ['error-snackbar']
        });
        this.isSubmitting.set(false);
      }
    });
  }
}