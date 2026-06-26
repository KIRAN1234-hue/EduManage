import {
  Component, OnInit, inject, signal
} from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { MatIconModule } from '@angular/material/icon';
import { MatButtonModule } from '@angular/material/button';
import { MatSelectModule } from '@angular/material/select';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';

import { AuditService } from '../../../core/services/audit.service';
import { AuditLogEntry } from '../../../core/models/audit.models';

@Component({
  selector: 'app-audit-log',
  standalone: true,
  imports: [
    CommonModule, FormsModule,
    MatIconModule, MatButtonModule,
    MatSelectModule, MatFormFieldModule,
    MatProgressSpinnerModule
  ],
  templateUrl: './audit-log.component.html',
  styleUrls: ['./audit-log.component.scss']
})
export class AuditLogComponent implements OnInit {
  private auditService = inject(AuditService);

  logs:         AuditLogEntry[] = [];
  total         = 0;
  page          = 1;
  pageSize      = 50;
  isLoading     = signal(false);
  filterEntity  = '';
  filterAction  = '';
  expandedId    = '';

  entities = [
    'Attendance', 'Marks', 'Assignment',
    'Leave', 'Fee', 'Library', 'Notice',
    'Complaint', 'Timetable', 'ExamSchedule'
  ];

  actions = ['Create', 'Update', 'Delete'];

  ngOnInit(): void {
    this.loadLogs();
  }

  loadLogs(): void {
    this.isLoading.set(true);
    this.auditService.getAuditLogs(
      this.page,
      this.pageSize,
      this.filterEntity || undefined,
      this.filterAction || undefined
    ).subscribe({
      next: res => {
        this.logs  = res.data;
        this.total = res.total;
        this.isLoading.set(false);
      },
      error: () => this.isLoading.set(false)
    });
  }

  applyFilters(): void {
    this.page = 1;
    this.loadLogs();
  }

  clearFilters(): void {
    this.filterEntity = '';
    this.filterAction = '';
    this.page         = 1;
    this.loadLogs();
  }

  goToPage(p: number): void {
    if (p < 1 || p > this.totalPages) return;
    this.page = p;
    this.loadLogs();
  }

  get totalPages(): number {
    return Math.ceil(this.total / this.pageSize);
  }

  toggleExpand(id: string): void {
    this.expandedId = this.expandedId === id ? '' : id;
  }

  getActionColor(action: string): string {
    return {
      Create: '#16A34A',
      Update: '#2563EB',
      Delete: '#DC2626'
    }[action] ?? '#64748B';
  }

  getActionBg(action: string): string {
    return {
      Create: '#DCFCE7',
      Update: '#DBEAFE',
      Delete: '#FEE2E2'
    }[action] ?? '#F1F5F9';
  }

  formatJson(json?: string): string {
    if (!json) return '—';
    try {
      return JSON.stringify(JSON.parse(json), null, 2);
    } catch {
      return json;
    }
  }
}