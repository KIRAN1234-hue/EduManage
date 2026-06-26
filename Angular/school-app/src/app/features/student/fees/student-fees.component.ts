import { Component, OnInit, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { MatIconModule } from '@angular/material/icon';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { FeeService } from '../../../core/services/fee.service';
import { StudentFeeStatus } from '../../../core/models/fee.models';

@Component({
  selector: 'app-student-fees',
  standalone: true,
  imports: [CommonModule, MatIconModule, MatProgressSpinnerModule],
  templateUrl: './student-fees.component.html',
  styleUrls: ['./student-fees.component.scss']
})
export class StudentFeesComponent implements OnInit {
  private feeService = inject(FeeService);

  feeStatus = signal<StudentFeeStatus | null>(null);
  isLoading = signal(true);

  ngOnInit(): void {
    this.feeService.getMyFeeStatus().subscribe({
      next: d => { this.feeStatus.set(d); this.isLoading.set(false); },
      error: () => this.isLoading.set(false)
    });
  }

  getStatusColor(s: string): string {
    return { Paid:'#16A34A', Pending:'#D97706', Overdue:'#DC2626', Partial:'#2563EB' }[s] ?? '#64748B';
  }

  getStatusBg(s: string): string {
    return { Paid:'#DCFCE7', Pending:'#FEF3C7', Overdue:'#FEE2E2', Partial:'#DBEAFE' }[s] ?? '#F1F5F9';
  }

  getProgressWidth(paid: number, total: number): string {
    return total > 0 ? `${Math.min(100, Math.round(paid / total * 100))}%` : '0%';
  }
}