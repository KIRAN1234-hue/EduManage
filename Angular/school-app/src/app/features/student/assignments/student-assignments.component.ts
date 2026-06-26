import {
  Component, OnInit, inject, signal
} from '@angular/core';
import { CommonModule } from '@angular/common';
import {
  ReactiveFormsModule, FormBuilder,
  FormGroup, Validators
} from '@angular/forms';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatSnackBar, MatSnackBarModule } from '@angular/material/snack-bar';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { Store } from '@ngrx/store';

import { AssignmentService } from '../../../core/services/assignment.service';
import { AdminService } from '../../../core/services/admin.service';
import {
  AssignmentResponse, SubmissionResponse
} from '../../../core/models/assignment.models';
import { selectCurrentUser } from '../../../core/store/auth/auth.selectors';

@Component({
  selector: 'app-student-assignments',
  standalone: true,
  imports: [
    CommonModule, ReactiveFormsModule,
    MatButtonModule, MatIconModule,
    MatFormFieldModule, MatInputModule,
    MatSnackBarModule, MatProgressSpinnerModule
  ],
  templateUrl: './student-assignments.component.html',
  styleUrls: ['./student-assignments.component.scss']
})
export class StudentAssignmentsComponent implements OnInit {
  private assignmentService = inject(AssignmentService);
  private adminService      = inject(AdminService);
  private snackBar          = inject(MatSnackBar);
  private store             = inject(Store);
  private fb                = inject(FormBuilder);

  user$ = this.store.select(selectCurrentUser);

  assignments:     AssignmentResponse[] = [];
  selectedAssignment = signal<AssignmentResponse | null>(null);
  mySubmission     = signal<SubmissionResponse | null>(null);
  isLoading        = signal(false);
  isSubmitting     = signal(false);
  showSubmitForm   = signal(false);

  submitForm!: FormGroup;

  ngOnInit(): void {
    this.submitForm = this.fb.group({
      content: ['', Validators.required]
    });
    this.loadAssignments();
  }

  private loadAssignments(): void {
    this.isLoading.set(true);
    // Will use student's classId from their profile
    // For now load using the student's class - this will be populated from student profile API
    // Using a general approach - the backend filters by the student's own context
    this.assignmentService.getMyClassAssignments().subscribe({
      next: data => {
        this.assignments = data;
        this.isLoading.set(false);
      },
      error: () => {
        // Fallback: try to load directly if getMyAssignments fails
        this.isLoading.set(false);
      }
    });
  }

  viewAssignment(assignment: AssignmentResponse): void {
    this.selectedAssignment.set(assignment);
    this.mySubmission.set(null);

    this.assignmentService.getMySubmission(assignment.id).subscribe({
      next: data => this.mySubmission.set(data)
    });
  }

  submitAssignment(): void {
    if (this.submitForm.invalid) return;
    const assignment = this.selectedAssignment();
    if (!assignment) return;

    this.isSubmitting.set(true);
    this.assignmentService.submitAssignment(assignment.id, this.submitForm.value).subscribe({
      next: (res) => {
        this.mySubmission.set(res);
        this.showSubmitForm.set(false);
        this.submitForm.reset();
        this.snackBar.open('Assignment submitted!', 'Close', {
          duration: 3000, panelClass: ['success-snackbar']
        });
        this.isSubmitting.set(false);
      },
      error: (err) => {
        this.snackBar.open(
          err.error?.message ?? 'Submission failed.',
          'Close', { duration: 4000, panelClass: ['error-snackbar'] }
        );
        this.isSubmitting.set(false);
      }
    });
  }

  backToList(): void {
    this.selectedAssignment.set(null);
    this.mySubmission.set(null);
    this.showSubmitForm.set(false);
  }

  getStatusColor(status: string): string {
    const m: Record<string,string> = {
      Submitted:'#2563EB', Late:'#D97706', Graded:'#16A34A'
    };
    return m[status] ?? '#64748B';
  }

  getStatusBg(status: string): string {
    const m: Record<string,string> = {
      Submitted:'#DBEAFE', Late:'#FEF3C7', Graded:'#DCFCE7'
    };
    return m[status] ?? '#F1F5F9';
  }
}