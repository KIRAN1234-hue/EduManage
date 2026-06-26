import {
  Component, OnInit, inject, signal
} from '@angular/core';
import { CommonModule } from '@angular/common';
import {
  ReactiveFormsModule, FormBuilder, FormGroup, Validators
} from '@angular/forms';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatSelectModule } from '@angular/material/select';
import { MatSnackBar, MatSnackBarModule } from '@angular/material/snack-bar';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';

import { ComplaintService } from '../../../core/services/complaint.service';
import { ComplaintResponse } from '../../../core/models/complaint.models';

@Component({
  selector: 'app-student-complaints',
  standalone: true,
  imports: [
    CommonModule, ReactiveFormsModule,
    MatButtonModule, MatIconModule,
    MatFormFieldModule, MatInputModule,
    MatSelectModule, MatSnackBarModule,
    MatProgressSpinnerModule
  ],
  templateUrl: './student-complaints.component.html',
  styleUrls: ['./student-complaints.component.scss']
})
export class StudentComplaintsComponent implements OnInit {
  private complaintService = inject(ComplaintService);
  private snackBar         = inject(MatSnackBar);
  private fb               = inject(FormBuilder);

  complaints:  ComplaintResponse[] = [];
  isLoading    = signal(false);
  isSubmitting = signal(false);
  showForm     = signal(false);

  form!: FormGroup;

  categories = [
    { value: 1, label: 'Academic'       },
    { value: 2, label: 'Infrastructure' },
    { value: 3, label: 'Teacher'        },
    { value: 4, label: 'Admin'          },
    { value: 5, label: 'Other'          },
  ];

  ngOnInit(): void {
    this.form = this.fb.group({
      title:       ['', Validators.required],
      category:    ['', Validators.required],
      description: ['', [Validators.required, Validators.minLength(20)]]
    });

    this.loadComplaints();
  }

  loadComplaints(): void {
    this.isLoading.set(true);
    this.complaintService.getMyComplaints().subscribe({
      next: d => { this.complaints = d; this.isLoading.set(false); },
      error: () => this.isLoading.set(false)
    });
  }

  submit(): void {
    if (this.form.invalid) { this.form.markAllAsTouched(); return; }
    this.isSubmitting.set(true);

    this.complaintService.submitComplaint(this.form.value).subscribe({
      next: (res) => {
        this.complaints.unshift(res);
        this.form.reset();
        this.showForm.set(false);
        this.snackBar.open('Complaint submitted!', 'Close', {
          duration: 3000, panelClass: ['success-snackbar']
        });
        this.isSubmitting.set(false);
      },
      error: () => this.isSubmitting.set(false)
    });
  }

  getStatusColor(s: string): string {
    return { Open:'#D97706', InReview:'#2563EB', Resolved:'#16A34A' }[s] ?? '#64748B';
  }

  getStatusBg(s: string): string {
    return { Open:'#FEF3C7', InReview:'#DBEAFE', Resolved:'#DCFCE7' }[s] ?? '#F1F5F9';
  }

  getStatusIcon(s: string): string {
    return { Open:'pending', InReview:'search', Resolved:'check_circle' }[s] ?? 'info';
  }
}