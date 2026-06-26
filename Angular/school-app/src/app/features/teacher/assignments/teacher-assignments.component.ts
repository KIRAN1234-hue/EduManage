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
import { MatDatepickerModule } from '@angular/material/datepicker';
import { MatNativeDateModule } from '@angular/material/core';
import { MatDialogModule } from '@angular/material/dialog';

import { AssignmentService } from '../../../core/services/assignment.service';
import { AdminService } from '../../../core/services/admin.service';
import {
  AssignmentResponse, SubmissionResponse,
  GradeSubmissionRequest
} from '../../../core/models/assignment.models';
import { ClassResponse, SubjectResponse } from '../../../core/models/admin.models';

@Component({
  selector: 'app-teacher-assignments',
  standalone: true,
  imports: [
    CommonModule, ReactiveFormsModule,
    MatTabsModule, MatFormFieldModule, MatInputModule,
    MatSelectModule, MatButtonModule, MatIconModule,
    MatSnackBarModule, MatProgressSpinnerModule,
    MatDatepickerModule, MatNativeDateModule, MatDialogModule
  ],
  templateUrl: './teacher-assignments.component.html',
  styleUrls: ['./teacher-assignments.component.scss']
})
export class TeacherAssignmentsComponent implements OnInit {
  private assignmentService = inject(AssignmentService);
  private adminService      = inject(AdminService);
  private snackBar          = inject(MatSnackBar);
  private fb                = inject(FormBuilder);

  createForm!: FormGroup;
  gradeForm!:  FormGroup;

  assignments: AssignmentResponse[] = [];
  submissions: SubmissionResponse[] = [];
  classes:     ClassResponse[]      = [];
  subjects:    SubjectResponse[]    = [];

  isLoadingAssignments = signal(false);
  isLoadingSubmissions = signal(false);
  isCreating           = signal(false);
  isGrading            = signal(false);
  showCreateForm       = signal(false);

  selectedAssignment   = signal<AssignmentResponse | null>(null);
  selectedSubmission   = signal<SubmissionResponse | null>(null);
  showGradePanel       = signal(false);

  ngOnInit(): void {
    this.initForms();
    this.loadData();
  }

  private initForms(): void {
    this.createForm = this.fb.group({
      title:      ['', Validators.required],
      description:['', Validators.required],
      dueDate:    ['', Validators.required],
      classId:    ['', Validators.required],
      subjectId:  ['', Validators.required],
      totalMarks: [10, [Validators.required, Validators.min(1)]]
    });

    this.gradeForm = this.fb.group({
      marksAwarded: ['', [Validators.required, Validators.min(0)]],
      teacherRemark:[''],
      feedback:     ['']
    });

    this.createForm.get('classId')?.valueChanges.subscribe(classId => {
      if (classId) {
        this.adminService.getSubjectsByClass(classId).subscribe({
          next: data => this.subjects = data
        });
      }
    });
  }

  private loadData(): void {
    this.isLoadingAssignments.set(true);
    this.adminService.getAllClasses().subscribe({
      next: data => this.classes = data
    });
    this.assignmentService.getMyAssignments().subscribe({
      next: data => {
        this.assignments = data;
        this.isLoadingAssignments.set(false);
      },
      error: () => this.isLoadingAssignments.set(false)
    });
  }

  createAssignment(): void {
    if (this.createForm.invalid) {
      this.createForm.markAllAsTouched(); return;
    }
    this.isCreating.set(true);
    const val = this.createForm.value;
    const dto = {
      ...val,
      dueDate: val.dueDate instanceof Date
        ? val.dueDate.toISOString()
        : val.dueDate
    };

    this.assignmentService.createAssignment(dto).subscribe({
      next: (res) => {
        this.assignments.unshift(res);
        this.createForm.reset({ totalMarks: 10 });
        this.showCreateForm.set(false);
        this.snackBar.open('Assignment created!', 'Close', {
          duration: 3000, panelClass: ['success-snackbar']
        });
        this.isCreating.set(false);
      },
      error: (err) => {
        this.snackBar.open(err.error?.message ?? 'Failed to create.', 'Close', {
          duration: 4000, panelClass: ['error-snackbar']
        });
        this.isCreating.set(false);
      }
    });
  }

  viewSubmissions(assignment: AssignmentResponse): void {
    this.selectedAssignment.set(assignment);
    this.isLoadingSubmissions.set(true);
    this.assignmentService.getSubmissions(assignment.id).subscribe({
      next: data => {
        this.submissions = data;
        this.isLoadingSubmissions.set(false);
      }
    });
  }

  openGradePanel(submission: SubmissionResponse): void {
    this.selectedSubmission.set(submission);
    this.gradeForm.patchValue({
      marksAwarded:  submission.marksAwarded ?? '',
      teacherRemark: submission.teacherRemark ?? '',
      feedback:      submission.feedback ?? ''
    });
    this.showGradePanel.set(true);
  }

  submitGrade(): void {
    if (this.gradeForm.invalid) return;
    const submission = this.selectedSubmission();
    if (!submission) return;

    this.isGrading.set(true);
    const dto: GradeSubmissionRequest = this.gradeForm.value;

    this.assignmentService.gradeSubmission(submission.id, dto).subscribe({
      next: (res) => {
        const idx = this.submissions.findIndex(s => s.id === submission.id);
        if (idx > -1) this.submissions[idx] = res;
        this.showGradePanel.set(false);
        this.snackBar.open('Graded successfully!', 'Close', {
          duration: 3000, panelClass: ['success-snackbar']
        });
        this.isGrading.set(false);
      },
      error: () => { this.isGrading.set(false); }
    });
  }

  getStatusColor(status: string): string {
    const map: Record<string, string> = {
      Submitted: '#2563EB', Late: '#D97706',
      Graded:    '#16A34A', Returned: '#7C3AED'
    };
    return map[status] ?? '#64748B';
  }

  getStatusBg(status: string): string {
    const map: Record<string, string> = {
      Submitted: '#DBEAFE', Late: '#FEF3C7',
      Graded:    '#DCFCE7', Returned: '#EDE9FE'
    };
    return map[status] ?? '#F1F5F9';
  }

  backToList(): void {
    this.selectedAssignment.set(null);
    this.submissions = [];
  }
}