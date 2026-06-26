import {
  Component, OnInit, inject, signal
} from '@angular/core';
import { CommonModule } from '@angular/common';
import {
  ReactiveFormsModule, FormBuilder,
  FormGroup, Validators, FormsModule
} from '@angular/forms';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatSelectModule } from '@angular/material/select';
import { MatInputModule } from '@angular/material/input';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatSnackBar, MatSnackBarModule } from '@angular/material/snack-bar';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatDatepickerModule } from '@angular/material/datepicker';
import { MatNativeDateModule } from '@angular/material/core';
import { Store } from '@ngrx/store';
import { selectUserRole } from '../../../core/store/auth/auth.selectors';

import { ExamScheduleService } from '../../../core/services/exam-schedule.service';
import { AdminService } from '../../../core/services/admin.service';
import { ExamScheduleResponse } from '../../../core/models/exam-schedule.models';
import { ClassResponse, SubjectResponse, TeacherResponse } from '../../../core/models/admin.models';

@Component({
  selector: 'app-exam-schedule',
  standalone: true,
  imports: [
    CommonModule, ReactiveFormsModule, FormsModule,
    MatFormFieldModule, MatSelectModule, MatInputModule,
    MatButtonModule, MatIconModule, MatSnackBarModule,
    MatProgressSpinnerModule, MatDatepickerModule, MatNativeDateModule
  ],
  templateUrl: './exam-schedule.component.html',
  styleUrls: ['./exam-schedule.component.scss']
})
export class ExamScheduleComponent implements OnInit {
  private examService  = inject(ExamScheduleService);
  private adminService = inject(AdminService);
  private snackBar     = inject(MatSnackBar);
  private store        = inject(Store);
  private fb           = inject(FormBuilder);

  role$    = this.store.select(selectUserRole);
  userRole = '';

  createForm!:  FormGroup;
  filterClassId = '';

  classes:  ClassResponse[]   = [];
  subjects: SubjectResponse[] = [];
  teachers: TeacherResponse[] = [];
  exams:    ExamScheduleResponse[] = [];

  isLoading  = signal(false);
  isCreating = signal(false);
  showCreate = signal(false);

  examTypes = [
    { value: 1, label: 'Unit Test'  },
    { value: 2, label: 'Mid Term'   },
    { value: 3, label: 'Final Exam' },
    { value: 4, label: 'Practical'  },
  ];

  ngOnInit(): void {
    this.createForm = this.fb.group({
      classId:              ['', Validators.required],
      subjectId:            ['', Validators.required],
      invigilatorTeacherId: ['', Validators.required],
      examType:             ['', Validators.required],
      examDate:             ['', Validators.required],
      startTime:            ['', Validators.required],
      endTime:              ['', Validators.required],
      roomNumber:           ['', Validators.required]
    });

   this.role$.subscribe(role => {
    this.userRole = role ?? '';

    if (role?.toLowerCase() === 'principal') {
      // Principal needs dropdowns
      this.adminService.getAllClasses().subscribe({ next: d => this.classes = d });
      this.adminService.getAllTeachers().subscribe({ next: d => this.teachers = d });

    } else if (role?.toLowerCase() === 'teacher') {
      // Teacher needs class list only
      this.adminService.getAllClasses().subscribe({ next: d => this.classes = d });

    } else if (role?.toLowerCase() === 'student') {
      // Student: auto-load their class exams — NO admin APIs
      this.isLoading.set(true);
      this.examService.getMyClassExams().subscribe({
        next: d => { this.exams = d; this.isLoading.set(false); },
        error: () => this.isLoading.set(false)
      });
    }
  });

  this.createForm.get('classId')?.valueChanges.subscribe(classId => {
    if (classId) this.adminService.getSubjectsByClass(classId)
      .subscribe({ next: d => this.subjects = d });
  });
}

  loadExams(): void {
    if (!this.filterClassId) return;
    this.isLoading.set(true);
    this.examService.getClassExams(this.filterClassId).subscribe({
      next: d => { this.exams = d; this.isLoading.set(false); },
      error: () => this.isLoading.set(false)
    });
  }

  createExam(): void {
    if (this.createForm.invalid) { this.createForm.markAllAsTouched(); return; }
    this.isCreating.set(true);

    const val = this.createForm.value;
    const dto = {
      ...val,
      examDate: val.examDate instanceof Date
        ? this.formatDate(val.examDate) : val.examDate
    };

    this.examService.createExam(dto).subscribe({
      next: (res) => {
        this.exams.push(res);
        this.exams.sort((a, b) => a.examDate > b.examDate ? 1 : -1);
        this.showCreate.set(false);
        this.snackBar.open('Exam scheduled!', 'Close', {
          duration: 3000, panelClass: ['success-snackbar']
        });
        this.isCreating.set(false);
      },
      error: (err) => {
        this.snackBar.open(err.error?.message ?? 'Failed.', 'Close', {
          duration: 4000, panelClass: ['error-snackbar']
        });
        this.isCreating.set(false);
      }
    });
  }

  toggleMarksEntry(exam: ExamScheduleResponse): void {
    this.examService.toggleMarksEntry(exam.id).subscribe({
      next: () => { exam.marksEntryOpen = !exam.marksEntryOpen; }
    });
  }

  deleteExam(id: string): void {
    if (!confirm('Delete this exam schedule?')) return;
    this.examService.deleteExam(id).subscribe({
      next: () => { this.exams = this.exams.filter(e => e.id !== id); }
    });
  }

  private formatDate(d: Date): string {
    const y = d.getFullYear();
    const m = String(d.getMonth()+1).padStart(2,'0');
    const day = String(d.getDate()).padStart(2,'0');
    return `${y}-${m}-${day}`;
  }

  isPrincipal(): boolean { return this.userRole.toLowerCase() === 'principal'; }

  getExamTypeColor(type: string): string {
    const map: Record<string,string> = {
      UnitTest:'#2563EB', MidTerm:'#D97706', Final:'#DC2626', Practical:'#16A34A'
    };
    return map[type] ?? '#64748B';
  }

  getExamTypeBg(type: string): string {
    const map: Record<string,string> = {
      UnitTest:'#DBEAFE', MidTerm:'#FEF3C7', Final:'#FEE2E2', Practical:'#DCFCE7'
    };
    return map[type] ?? '#F1F5F9';
  }
}