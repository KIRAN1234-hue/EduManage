import {
  Component, OnInit, inject, signal
} from '@angular/core';
import { CommonModule } from '@angular/common';
import {
  ReactiveFormsModule, FormBuilder,
  FormGroup, Validators
} from '@angular/forms';
import { MatTabsModule } from '@angular/material/tabs';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatSelectModule } from '@angular/material/select';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatSnackBar, MatSnackBarModule } from '@angular/material/snack-bar';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatCheckboxModule } from '@angular/material/checkbox';
import { MatTooltipModule } from '@angular/material/tooltip';
import { MatDatepickerModule } from '@angular/material/datepicker';
import { MatNativeDateModule } from '@angular/material/core';
import { AdminService } from '../../../core/services/admin.service';
import {
  ClassResponse, TeacherResponse,
  StudentResponse, SubjectResponse
} from '../../../core/models/admin.models';

@Component({
  selector: 'app-user-management',
  standalone: true,
  imports: [
    CommonModule, ReactiveFormsModule,
    MatTabsModule, MatFormFieldModule,
    MatInputModule, MatSelectModule,
    MatButtonModule, MatIconModule,
    MatSnackBarModule, MatProgressSpinnerModule,
    MatCheckboxModule, MatTooltipModule,
    MatDatepickerModule, MatNativeDateModule
  ],
  templateUrl: './user-management.component.html',
  styleUrls: ['./user-management.component.scss']
})
export class UserManagementComponent implements OnInit {
  private adminService = inject(AdminService);
  private snackBar     = inject(MatSnackBar);
  private fb           = inject(FormBuilder);

  // Data lists
  classes:  ClassResponse[]   = [];
  teachers: TeacherResponse[] = [];
  students: StudentResponse[] = [];
  subjects: SubjectResponse[] = [];

  // Loading states
  isLoadingClasses  = signal(false);
  isLoadingTeachers = signal(false);
  isLoadingStudents = signal(false);
  isLoadingSubjects = signal(false);

  // Forms
  classForm!:   FormGroup;
  subjectForm!: FormGroup;
  teacherForm!: FormGroup;
  studentForm!: FormGroup;

  // Submitting states
  isSubmittingClass   = signal(false);
  isSubmittingSubject = signal(false);
  isSubmittingTeacher = signal(false);
  isSubmittingStudent = signal(false);

  // After creation — show invite token
  lastInviteToken   = signal<string | null>(null);
  lastInviteEmail   = signal<string | null>(null);
  lastCreatedRole   = signal<string>('');

  showCreateClassForm   = signal(false);
  showCreateSubjectForm = signal(false);
  showCreateTeacherForm = signal(false);
  showCreateStudentForm = signal(false);

  ngOnInit(): void {
    this.initForms();
    this.loadAll();
  }

  private initForms(): void {
    this.classForm = this.fb.group({
      name:         ['', Validators.required],
      section:      ['', Validators.required],
      academicYear: ['2024-25', Validators.required],
      maxStrength:  [40, [Validators.required, Validators.min(1)]]
    });

    this.subjectForm = this.fb.group({
      name:       ['', Validators.required],
      code:       ['', Validators.required],
      maxMarks:   [100, [Validators.required, Validators.min(1)]],
      isElective: [false],
      classId:    ['', Validators.required],
      teacherId:  ['']
    });

    this.teacherForm = this.fb.group({
      fullName:      ['', Validators.required],
      email:         ['', [Validators.required, Validators.email]],
      qualification: ['', Validators.required],
      employeeCode:  ['', Validators.required],
      joiningDate:   ['', Validators.required],
      classId:       [''],
      isClassTeacher:[false]
    });

    this.studentForm = this.fb.group({
      fullName:      ['', Validators.required],
      email:         ['', [Validators.required, Validators.email]],
      rollNumber:    ['', Validators.required],
      classId:       ['', Validators.required],
      dateOfBirth:   ['', Validators.required],
      admissionDate: ['', Validators.required]
    });
  }

  private loadAll(): void {
    this.loadClasses();
    this.loadTeachers();
    this.loadStudents();
    this.loadSubjects();
  }

  loadClasses(): void {
    this.isLoadingClasses.set(true);
    this.adminService.getAllClasses().subscribe({
      next: data => { this.classes = data; this.isLoadingClasses.set(false); },
      error: ()  => this.isLoadingClasses.set(false)
    });
  }

  loadTeachers(): void {
    this.isLoadingTeachers.set(true);
    this.adminService.getAllTeachers().subscribe({
      next: data => { this.teachers = data; this.isLoadingTeachers.set(false); },
      error: ()  => this.isLoadingTeachers.set(false)
    });
  }

  loadStudents(): void {
    this.isLoadingStudents.set(true);
    this.adminService.getAllStudents().subscribe({
      next: data => { this.students = data; this.isLoadingStudents.set(false); },
      error: ()  => this.isLoadingStudents.set(false)
    });
  }

  loadSubjects(): void {
    this.isLoadingSubjects.set(true);
    this.adminService.getAllSubjects().subscribe({
      next: data => { this.subjects = data; this.isLoadingSubjects.set(false); },
      error: ()  => this.isLoadingSubjects.set(false)
    });
  }

  // ── Create Class ──────────────────────────────────────────────────────────
  createClass(): void {
    if (this.classForm.invalid) { this.classForm.markAllAsTouched(); return; }
    this.isSubmittingClass.set(true);

    this.adminService.createClass(this.classForm.value).subscribe({
      next: (res) => {
        this.classes.unshift(res);
        this.classForm.reset({ academicYear: '2024-25', maxStrength: 40 });
        this.showCreateClassForm.set(false);
        this.showSuccess('Class created successfully.');
        this.isSubmittingClass.set(false);
      },
      error: (err) => {
        this.showError(err.error?.message ?? 'Failed to create class.');
        this.isSubmittingClass.set(false);
      }
    });
  }

  // ── Create Subject ────────────────────────────────────────────────────────
  createSubject(): void {
    if (this.subjectForm.invalid) { this.subjectForm.markAllAsTouched(); return; }
    this.isSubmittingSubject.set(true);

    const val = this.subjectForm.value;
    const dto = { ...val, teacherId: val.teacherId || undefined };

    this.adminService.createSubject(dto).subscribe({
      next: (res) => {
        this.subjects.unshift(res);
        this.subjectForm.reset({ maxMarks: 100, isElective: false });
        this.showCreateSubjectForm.set(false);
        this.showSuccess('Subject created successfully.');
        this.isSubmittingSubject.set(false);
      },
      error: (err) => {
        this.showError(err.error?.message ?? 'Failed to create subject.');
        this.isSubmittingSubject.set(false);
      }
    });
  }

  // ── Create Teacher ────────────────────────────────────────────────────────
  createTeacher(): void {
    if (this.teacherForm.invalid) { this.teacherForm.markAllAsTouched(); return; }
    this.isSubmittingTeacher.set(true);

    const val = this.teacherForm.value;
    const joiningDate = val.joiningDate instanceof Date
      ? val.joiningDate.toISOString().split('T')[0]
      : val.joiningDate;

    const dto = {
      ...val,
      joiningDate,
      classId: val.classId || undefined
    };

    this.adminService.createTeacher(dto).subscribe({
      next: (res) => {
        this.teachers.unshift(res);
        this.lastInviteToken.set(res.inviteToken ?? null);
        this.lastInviteEmail.set(res.email);
        this.lastCreatedRole.set('Teacher');
        this.teacherForm.reset({ isClassTeacher: false });
        this.showCreateTeacherForm.set(false);
        this.loadClasses(); // refresh class teacher info
        this.isSubmittingTeacher.set(false);
      },
      error: (err) => {
        this.showError(err.error?.message ?? 'Failed to create teacher.');
        this.isSubmittingTeacher.set(false);
      }
    });
  }

  // ── Create Student ────────────────────────────────────────────────────────
  createStudent(): void {
    if (this.studentForm.invalid) { this.studentForm.markAllAsTouched(); return; }
    this.isSubmittingStudent.set(true);

    const val = this.studentForm.value;
    const dateOfBirth = val.dateOfBirth instanceof Date
      ? val.dateOfBirth.toISOString().split('T')[0]
      : val.dateOfBirth;
    const admissionDate = val.admissionDate instanceof Date
      ? val.admissionDate.toISOString().split('T')[0]
      : val.admissionDate;

    const dto = { ...val, dateOfBirth, admissionDate };

    this.adminService.createStudent(dto).subscribe({
      next: (res) => {
        this.students.unshift(res);
        this.lastInviteToken.set(res.inviteToken ?? null);
        this.lastInviteEmail.set(res.email);
        this.lastCreatedRole.set('Student');
        this.studentForm.reset();
        this.showCreateStudentForm.set(false);
        this.isSubmittingStudent.set(false);
      },
      error: (err) => {
        this.showError(err.error?.message ?? 'Failed to create student.');
        this.isSubmittingStudent.set(false);
      }
    });
  }

  copyToClipboard(text: string): void {
    navigator.clipboard.writeText(text);
    this.showSuccess('Copied to clipboard.');
  }

  clearInviteToken(): void {
    this.lastInviteToken.set(null);
    this.lastInviteEmail.set(null);
  }

  private showSuccess(msg: string): void {
    this.snackBar.open(msg, 'Close', {
      duration: 4000,
      panelClass: ['success-snackbar'],
      horizontalPosition: 'center',
      verticalPosition: 'top'
    });
  }

  private showError(msg: string): void {
    this.snackBar.open(msg, 'Close', {
      duration: 5000,
      panelClass: ['error-snackbar'],
      horizontalPosition: 'center',
      verticalPosition: 'top'
    });
  }
  // Add confirm delete state
confirmDeleteId = signal<string | null>(null);
confirmDeleteType = signal<string>('');

confirmDelete(id: string, type: string): void {
  this.confirmDeleteId.set(id);
  this.confirmDeleteType.set(type);
}

cancelDelete(): void {
  this.confirmDeleteId.set(null);
  this.confirmDeleteType.set('');
}

executeDelete(): void {
  const id   = this.confirmDeleteId();
  const type = this.confirmDeleteType();
  if (!id) return;

  const call$ = type === 'class'   ? this.adminService.deleteClass(id)
              : type === 'subject' ? this.adminService.deleteSubject(id)
              : type === 'teacher' ? this.adminService.deleteTeacher(id)
              :                      this.adminService.deleteStudent(id);

  call$.subscribe({
    next: (res) => {
      if (type === 'class')   this.classes  = this.classes.filter(c => c.id !== id);
      if (type === 'subject') this.subjects = this.subjects.filter(s => s.id !== id);
      if (type === 'teacher') this.teachers = this.teachers.filter(t => t.id !== id);
      if (type === 'student') this.students = this.students.filter(s => s.id !== id);
      this.cancelDelete();
      this.showSuccess(res.message);
    },
    error: (err) => {
      this.showError(err.error?.message ?? 'Delete failed.');
      this.cancelDelete();
    }
  });
}
}