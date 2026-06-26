import {
  Component, OnInit, inject, signal
} from '@angular/core';
import { CommonModule } from '@angular/common';
import {
  ReactiveFormsModule, FormBuilder, FormGroup
} from '@angular/forms';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatSnackBar, MatSnackBarModule } from '@angular/material/snack-bar';

import { LeaveService } from '../../../core/services/leave.service';
import { LeaveResponse } from '../../../core/models/leave.models';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';

@Component({
  selector: 'app-leave-approvals',
  standalone: true,
  imports: [
    CommonModule, ReactiveFormsModule,
    MatButtonModule, MatIconModule,
    MatFormFieldModule, MatInputModule,
    MatSnackBarModule,MatProgressSpinnerModule
  ],
  templateUrl: './leave-approvals.component.html',
  styleUrls: ['./leave-approvals.component.scss']
})
export class LeaveApprovalsComponent implements OnInit {
  private leaveService = inject(LeaveService);
  private snackBar     = inject(MatSnackBar);
  private fb           = inject(FormBuilder);

  leaves    = signal<LeaveResponse[]>([]);
  isLoading = signal(false);

  processingId   = signal<string | null>(null);
  remarkForms:   Record<string, FormGroup> = {};

  ngOnInit(): void {
    this.loadPending();
  }

  loadPending(): void {
    this.isLoading.set(true);
    this.leaveService.getPendingLeaves().subscribe({
      next: data => {
        this.leaves.set(data);
        data.forEach(l => {
          this.remarkForms[l.id] = this.fb.group({ approvalRemark: [''] });
        });
        this.isLoading.set(false);
      },
      error: () => this.isLoading.set(false)
    });
  }

  processLeave(leave: LeaveResponse, isApproved: boolean): void {
    this.processingId.set(leave.id);
    const remark = this.remarkForms[leave.id]?.value.approvalRemark ?? '';

    this.leaveService.processLeave(leave.id, {
      isApproved,
      approvalRemark: remark || undefined
    }).subscribe({
      next: () => {
        this.leaves.update(ls => ls.filter(l => l.id !== leave.id));
        this.snackBar.open(
          isApproved ? 'Leave Approved.' : 'Leave Rejected.',
          'Close',
          { duration: 3000, panelClass: [isApproved ? 'success-snackbar' : 'error-snackbar'] }
        );
        this.processingId.set(null);
      },
      error: (err) => {
        this.snackBar.open(err.error?.message ?? 'Failed.', 'Close', { duration: 4000 });
        this.processingId.set(null);
      }
    });
  }
}