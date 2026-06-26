import {
  Component, OnInit, inject, signal
} from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule, ReactiveFormsModule } from '@angular/forms';
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
  selector: 'app-complaints-management',
  standalone: true,
  imports: [
    CommonModule, FormsModule, ReactiveFormsModule,
    MatButtonModule, MatIconModule,
    MatFormFieldModule, MatInputModule,
    MatSelectModule, MatSnackBarModule,
    MatProgressSpinnerModule
  ],
  templateUrl: './complaints-management.component.html',
  styleUrls: ['./complaints-management.component.scss']
})
export class ComplaintsManagementComponent implements OnInit {
  private complaintService = inject(ComplaintService);
  private snackBar         = inject(MatSnackBar);

  complaints:     ComplaintResponse[] = [];
  isLoading       = signal(false);
  isUpdating      = signal<string | null>(null);
  selectedStatus  = 'all';
  resolutionInput: Record<string, string> = {};

  statusFilters = [
    { value: 'all',      label: 'All'       },
    { value: 'Open',     label: 'Open'      },
    { value: 'InReview', label: 'In Review' },
    { value: 'Resolved', label: 'Resolved'  },
  ];

  ngOnInit(): void { this.loadComplaints(); }

  loadComplaints(): void {
    this.isLoading.set(true);
    const status = this.selectedStatus === 'all' ? undefined : this.selectedStatus;

    this.complaintService.getAllComplaints(status).subscribe({
      next: d => { this.complaints = d; this.isLoading.set(false); },
      error: () => this.isLoading.set(false)
    });
  }

  updateStatus(complaint: ComplaintResponse, newStatus: string): void {
    this.isUpdating.set(complaint.id);
    this.complaintService.updateComplaint(complaint.id, {
      newStatus,
      resolutionRemark: this.resolutionInput[complaint.id]
    }).subscribe({
      next: (updated) => {
        const idx = this.complaints.findIndex(c => c.id === complaint.id);
        if (idx > -1) this.complaints[idx] = updated;
        this.snackBar.open('Status updated!', 'Close', {
          duration: 3000, panelClass: ['success-snackbar']
        });
        this.isUpdating.set(null);
      },
      error: (err) => {
        this.snackBar.open(err.error?.message ?? 'Failed.', 'Close', {
          duration: 4000, panelClass: ['error-snackbar']
        });
        this.isUpdating.set(null);
      }
    });
  }

  getStatusColor(s: string): string {
    return { Open:'#D97706', InReview:'#2563EB', Resolved:'#16A34A' }[s] ?? '#64748B';
  }

  getStatusBg(s: string): string {
    return { Open:'#FEF3C7', InReview:'#DBEAFE', Resolved:'#DCFCE7' }[s] ?? '#F1F5F9';
  }

    // ADD this method:
getStatusCount(status: string): number {
  return this.complaints.filter(c => c.status === status).length;
}
}