import {
  Component, OnInit, inject, signal
} from '@angular/core';
import { CommonModule } from '@angular/common';
import {
  ReactiveFormsModule, FormBuilder,
  FormGroup, FormsModule, Validators
} from '@angular/forms';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatSelectModule } from '@angular/material/select';
import { MatInputModule } from '@angular/material/input';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatSnackBar, MatSnackBarModule } from '@angular/material/snack-bar';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatTooltipModule } from '@angular/material/tooltip';

import { MarksService } from '../../../core/services/marks.service';
import { AdminService } from '../../../core/services/admin.service';

import { CreateMarkRequest } from '../../../core/models/marks.models';
import {
  ClassResponse,
  SubjectResponse
} from '../../../core/models/admin.models';

interface StudentMarkRow {
  studentId: string;
  studentName: string;
  rollNumber: string;
  marksObtained: number | null;
  grade: string;
  percentage: number;
}

@Component({
  selector: 'app-teacher-marks',
  standalone: true,
  imports: [
    CommonModule,
    ReactiveFormsModule,
    FormsModule,
    MatFormFieldModule,
    MatSelectModule,
    MatInputModule,
    MatButtonModule,
    MatIconModule,
    MatSnackBarModule,
    MatProgressSpinnerModule,
    MatTooltipModule
  ],
  templateUrl: './teacher-marks.component.html',
  styleUrls: ['./teacher-marks.component.scss']
})
export class TeacherMarksComponent implements OnInit {

  private marksService = inject(MarksService);
  private adminService = inject(AdminService);
  private snackBar     = inject(MatSnackBar);
  private fb           = inject(FormBuilder);

  filterForm!: FormGroup;

  isLoading      = signal(false);
  isSubmitting   = signal(false);
  studentsLoaded = signal(false);

  students: StudentMarkRow[] = [];

  // REAL API DATA
  classes: ClassResponse[] = [];
  subjects: SubjectResponse[] = [];

  maxMarks = 100;

  examTypes = [
    { value: 'UnitTest',  label: 'Unit Test'  },
    { value: 'MidTerm',   label: 'Mid Term'   },
    { value: 'Final',     label: 'Final Exam' },
    { value: 'Practical', label: 'Practical'  },
  ];

  ngOnInit(): void {

    this.filterForm = this.fb.group({
      classId:   ['', Validators.required],
      subjectId: ['', Validators.required],
      examType:  ['', Validators.required],
      maxMarks:  [100, [Validators.required, Validators.min(1)]]
    });

    // Load real classes from API
    this.adminService.getAllClasses().subscribe({
      next: data => this.classes = data
    });

    this.filterForm.get('maxMarks')?.valueChanges.subscribe(val => {

      this.maxMarks = val;

      this.students.forEach(s => {
        if (s.marksObtained !== null) {
          this.recalculate(s);
        }
      });

    });
  }

  onClassChange(): void {

    const classId = this.filterForm.value.classId;

    this.subjects = [];

    this.filterForm.patchValue({
      subjectId: ''
    });

    this.studentsLoaded.set(false);
    this.students = [];

    if (!classId) return;

    this.adminService.getSubjectsByClass(classId).subscribe({
      next: data => this.subjects = data
    });
  }

  loadStudents(): void {

    const { classId, subjectId, examType } = this.filterForm.value;

    if (!classId || !subjectId || !examType) return;

    this.isLoading.set(true);
    this.studentsLoaded.set(false);

    // Load REAL students from API
    this.adminService.getStudentsByClass(classId).subscribe({

      next: (data) => {

        this.students = data.map(s => ({
          studentId: s.id,
          studentName: s.fullName,
          rollNumber: s.rollNumber,
          marksObtained: null,
          grade: '',
          percentage: 0
        }));

        if (this.students.length === 0) {

          this.snackBar.open(
            'No students found in this class.',
            'Close',
            { duration: 3000 }
          );
        }

        this.isLoading.set(false);
        this.studentsLoaded.set(true);
      },

      error: () => {

        this.snackBar.open(
          'Failed to load students.',
          'Close',
          { duration: 3000 }
        );

        this.isLoading.set(false);
      }
    });
  }

  onMarksInput(student: StudentMarkRow): void {

    if (
      student.marksObtained !== null &&
      student.marksObtained !== undefined
    ) {

      // Clamp to max
      if (student.marksObtained > this.maxMarks) {
        student.marksObtained = this.maxMarks;
      }

      if (student.marksObtained < 0) {
        student.marksObtained = 0;
      }

      this.recalculate(student);

    } else {

      student.grade = '';
      student.percentage = 0;
    }
  }

  private recalculate(student: StudentMarkRow): void {

    const pct =
      (student.marksObtained! / this.maxMarks) * 100;

    student.percentage =
      Math.round(pct * 100) / 100;

    student.grade =
      this.calculateGrade(student.percentage);
  }

  calculateGrade(percentage: number): string {

    if (percentage >= 90) return 'A+';
    if (percentage >= 80) return 'A';
    if (percentage >= 70) return 'B';
    if (percentage >= 60) return 'C';
    if (percentage >= 50) return 'D';

    return 'F';
  }

  getGradeColor(grade: string): string {

    const map: Record<string, string> = {
      'A+': '#16A34A',
      'A':  '#22C55E',
      'B':  '#2563EB',
      'C':  '#D97706',
      'D':  '#EA580C',
      'F':  '#DC2626',
    };

    return map[grade] ?? '#94A3B8';
  }

  getGradeBg(grade: string): string {

    const map: Record<string, string> = {
      'A+': '#DCFCE7',
      'A':  '#F0FDF4',
      'B':  '#DBEAFE',
      'C':  '#FEF3C7',
      'D':  '#FFEDD5',
      'F':  '#FEE2E2',
    };

    return map[grade] ?? '#F1F5F9';
  }

  getEnteredCount(): number {

    return this.students.filter(
      s => s.marksObtained !== null
    ).length;
  }

  getClassAverage(): number {

    const entered = this.students.filter(
      s => s.marksObtained !== null
    );

    if (entered.length === 0) return 0;

    return Math.round(
      entered.reduce(
        (sum, s) => sum + s.percentage,
        0
      ) / entered.length
    );
  }

  submitMarks(): void {

    if (this.filterForm.invalid) {

      this.filterForm.markAllAsTouched();
      return;
    }

    const { subjectId, examType } =
      this.filterForm.value;

    const filledStudents =
      this.students.filter(
        s => s.marksObtained !== null
      );

    if (filledStudents.length === 0) {

      this.snackBar.open(
        'Please enter marks for at least one student.',
        'Close',
        { duration: 3000 }
      );

      return;
    }

    const records: CreateMarkRequest[] =
      filledStudents.map(s => ({
        studentId:     s.studentId,
        subjectId,
        examType,
        marksObtained: s.marksObtained!,
        maxMarks:      this.maxMarks
      }));

    this.isSubmitting.set(true);

    this.marksService.createBulkMarks(records).subscribe({

      next: (res) => {

        this.snackBar.open(
          res.message,
          'Close',
          {
            duration: 4000,
            panelClass: ['success-snackbar'],
            horizontalPosition: 'center',
            verticalPosition: 'top'
          }
        );

        this.isSubmitting.set(false);
      },

      error: (err) => {

        const msg =
          err.error?.message ??
          'Failed to submit marks.';

        this.snackBar.open(
          msg,
          'Close',
          {
            duration: 4000,
            panelClass: ['error-snackbar']
          }
        );

        this.isSubmitting.set(false);
      }
    });
  }
}