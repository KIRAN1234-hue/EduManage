import {
  Component, OnInit, inject, signal
} from '@angular/core';
import { CommonModule } from '@angular/common';
import {
  ReactiveFormsModule, FormBuilder,
  FormGroup, Validators
} from '@angular/forms';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatSelectModule } from '@angular/material/select';
import { MatInputModule } from '@angular/material/input';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatSnackBar, MatSnackBarModule } from '@angular/material/snack-bar';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { Store } from '@ngrx/store';
import { selectUserRole } from '../../../core/store/auth/auth.selectors';

import { TimetableService } from '../../../core/services/timetable.service';
import { AdminService } from '../../../core/services/admin.service';
import { TimetableResponse } from '../../../core/models/timetable.models';
import { ClassResponse, SubjectResponse, TeacherResponse } from '../../../core/models/admin.models';

@Component({
  selector: 'app-timetable',
  standalone: true,
  imports: [
    CommonModule, ReactiveFormsModule,
    MatFormFieldModule, MatSelectModule, MatInputModule,
    MatButtonModule, MatIconModule, MatSnackBarModule,
    MatProgressSpinnerModule
  ],
  templateUrl: './timetable.component.html',
  styleUrls: ['./timetable.component.scss']
})
export class TimetableComponent implements OnInit {
  private timetableService = inject(TimetableService);
  private adminService     = inject(AdminService);
  private snackBar         = inject(MatSnackBar);
  private store            = inject(Store);
  private fb               = inject(FormBuilder);

  role$    = this.store.select(selectUserRole);
  userRole = '';

  createForm!: FormGroup;
  filterClassId = '';

  classes:   ClassResponse[]   = [];
  subjects:  SubjectResponse[] = [];
  teachers:  TeacherResponse[] = [];
  timetable: TimetableResponse[] = [];

  isLoading    = signal(false);
  isCreating   = signal(false);
  showCreate   = signal(false);

  days = [
    { value: 1, label: 'Monday'    },
    { value: 2, label: 'Tuesday'   },
    { value: 3, label: 'Wednesday' },
    { value: 4, label: 'Thursday'  },
    { value: 5, label: 'Friday'    },
    { value: 6, label: 'Saturday'  },
  ];

  periods = [1, 2, 3, 4, 5, 6, 7, 8];

  // Group timetable by day for display
  get groupedByDay(): Map<string, TimetableResponse[]> {
    const map = new Map<string, TimetableResponse[]>();
    this.timetable.forEach(slot => {
      const existing = map.get(slot.dayOfWeek) ?? [];
      existing.push(slot);
      map.set(slot.dayOfWeek, existing);
    });
    return map;
  }

  get orderedDays(): string[] {
    const order = ['Monday','Tuesday','Wednesday','Thursday','Friday','Saturday'];
    return order.filter(d => this.groupedByDay.has(d));
  }

  ngOnInit(): void {
    this.createForm = this.fb.group({
      classId:      ['', Validators.required],
      subjectId:    ['', Validators.required],
      teacherId:    ['', Validators.required],
      dayOfWeek:    ['', Validators.required],
      periodNumber: ['', Validators.required],
      startTime:    ['', Validators.required],
      endTime:      ['', Validators.required],
      academicYear: ['2024-25', Validators.required]
    });

    this.role$.subscribe(role => {
    this.userRole = role ?? '';

    if (role?.toLowerCase() === 'principal') {
      // Principal needs dropdowns for creating timetable
      this.adminService.getAllClasses().subscribe({ next: d => this.classes = d });
      this.adminService.getAllTeachers().subscribe({ next: d => this.teachers = d });

    } else if (role?.toLowerCase() === 'teacher') {
      // Teacher only needs class list (they CAN access this)
      this.adminService.getAllClasses().subscribe({ next: d => this.classes = d });
      // Auto-load teacher's own timetable
      this.isLoading.set(true);
      this.timetableService.getMyTimetable().subscribe({
        next: d => { this.timetable = d; this.isLoading.set(false); },
        error: () => this.isLoading.set(false)
      });

    } else if (role?.toLowerCase() === 'student') {
      // Student needs NO admin APIs — auto-load their class timetable
      this.isLoading.set(true);
      this.timetableService.getMyClassTimetable().subscribe({
        next: d => { this.timetable = d; this.isLoading.set(false); },
        error: () => this.isLoading.set(false)
      });
    }
  });

  this.createForm.get('classId')?.valueChanges.subscribe(classId => {
    if (classId) {
      this.adminService.getSubjectsByClass(classId).subscribe({
        next: d => this.subjects = d
      });   
    }
  });
}

  loadTimetable(): void {
    if (!this.filterClassId) return;
    this.isLoading.set(true);
    this.timetableService.getClassTimetable(this.filterClassId).subscribe({
      next: d => { this.timetable = d; this.isLoading.set(false); },
      error: () => this.isLoading.set(false)
    });
  }

  loadMyTimetable(): void {
  this.isLoading.set(true);

  if (this.userRole.toLowerCase() === 'student') {
    this.timetableService.getMyClassTimetable().subscribe({
      next: d => {
        this.timetable = d;
        this.isLoading.set(false);
      },
      error: () => this.isLoading.set(false)
    });
  } else {
    this.timetableService.getMyTimetable().subscribe({
      next: d => {
        this.timetable = d;
        this.isLoading.set(false);
      },
      error: () => this.isLoading.set(false)
    });
  }
}

  createSlot(): void {
    if (this.createForm.invalid) { this.createForm.markAllAsTouched(); return; }
    this.isCreating.set(true);

    this.timetableService.createSlot(this.createForm.value).subscribe({
      next: (res) => {
        this.timetable.push(res);
        this.createForm.patchValue({ subjectId:'', teacherId:'', dayOfWeek:'', periodNumber:'', startTime:'', endTime:'' });
        this.showCreate.set(false);
        this.snackBar.open('Slot added!', 'Close', { duration: 3000, panelClass: ['success-snackbar'] });
        this.isCreating.set(false);
      },
      error: (err) => {
        this.snackBar.open(err.error?.message ?? 'Failed.', 'Close', { duration: 4000, panelClass: ['error-snackbar'] });
        this.isCreating.set(false);
      }
    });
  }

  deleteSlot(id: string): void {
    if (!confirm('Remove this slot?')) return;
    this.timetableService.deleteSlot(id).subscribe({
      next: () => { this.timetable = this.timetable.filter(t => t.id !== id); }
    });
  }

  isPrincipal(): boolean { return this.userRole.toLowerCase() === 'principal'; }
}