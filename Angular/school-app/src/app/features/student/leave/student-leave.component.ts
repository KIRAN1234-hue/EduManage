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
import { MatSelectModule } from '@angular/material/select';
import { MatDatepickerModule } from '@angular/material/datepicker';
import { MatNativeDateModule } from '@angular/material/core';
import { MatSnackBar, MatSnackBarModule } from '@angular/material/snack-bar';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';

import { LeaveService } from '../../../core/services/leave.service';
import { LeaveResponse } from '../../../core/models/leave.models';

@Component({
  selector: 'app-student-leave',
  standalone: true,
  imports: [
    CommonModule, ReactiveFormsModule,
    MatButtonModule, MatIconModule,
    MatFormFieldModule, MatInputModule,
    MatSelectModule, MatDatepickerModule,
    MatNativeDateModule, MatSnackBarModule,
    MatProgressSpinnerModule
  ],
  templateUrl: './student-leave.component.html',
  styleUrls: ['./student-leave.component.scss']
})
export class StudentLeaveComponent implements OnInit {
  private leaveService = inject(LeaveService);
  private snackBar     = inject(MatSnackBar);
  private fb           = inject(FormBuilder);

  leaves:       LeaveResponse[] = [];
  isLoading     = signal(false);
  isApplying    = signal(false);
  showApplyForm = signal(false);

  applyForm!: FormGroup;

  leaveTypes = [
    'Sick Leave', 'Casual Leave',
    'Emergency Leave', 'Other'
  ];

  ngOnInit(): void {
    this.applyForm = this.fb.group({
      leaveType: ['', Validators.required],
      fromDate:  ['', Validators.required],
      toDate:    ['', Validators.required],
      reason:    ['', Validators.required],
      remarks:   ['']
    });
    this.loadLeaves();
  }

  loadLeaves(): void {
    this.isLoading.set(true);
    this.leaveService.getMyLeaves().subscribe({
      next: data => { this.leaves = data; this.isLoading.set(false); },
      error: ()  => this.isLoading.set(false)
    });
  }

  applyLeave(): void {
    if (this.applyForm.invalid) {
      this.applyForm.markAllAsTouched(); return;
    }
    this.isApplying.set(true);

    const val = this.applyForm.value;
    const dto = {
      ...val,
      fromDate: val.fromDate instanceof Date
        ? this.formatDate(val.fromDate) : val.fromDate,
      toDate: val.toDate instanceof Date
        ? this.formatDate(val.toDate) : val.toDate
    };

    this.leaveService.applyLeave(dto).subscribe({
      next: (res) => {
        this.leaves.unshift(res);
        this.applyForm.reset();
        this.showApplyForm.set(false);
        this.snackBar.open('Leave application submitted!', 'Close', {
          duration: 3000, panelClass: ['success-snackbar']
        });
        this.isApplying.set(false);
      },
      error: (err) => {
        this.snackBar.open(err.error?.message ?? 'Failed to apply.', 'Close', {
          duration: 4000, panelClass: ['error-snackbar']
        });
        this.isApplying.set(false);
      }
    });
  }

  private formatDate(d: Date): string {
    const y = d.getFullYear();
    const m = String(d.getMonth()+1).padStart(2,'0');
    const day = String(d.getDate()).padStart(2,'0');
    return `${y}-${m}-${day}`;
  }

  getStatusColor(s: string): string {
    return { Pending:'#D97706', Approved:'#16A34A', Rejected:'#DC2626' }[s] ?? '#64748B';
  }

  getStatusBg(s: string): string {
    return { Pending:'#FEF3C7', Approved:'#DCFCE7', Rejected:'#FEE2E2' }[s] ?? '#F1F5F9';
  }
}