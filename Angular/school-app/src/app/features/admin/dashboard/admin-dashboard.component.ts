import {
  Component, OnInit, AfterViewInit,
  inject, ViewChild, ElementRef
} from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router } from '@angular/router';
import { Store } from '@ngrx/store';
import { MatIconModule } from '@angular/material/icon';
import { MatButtonModule } from '@angular/material/button';
import { Chart, registerables } from 'chart.js';
import { DashboardService } from '../../../core/services/dashboard.service';
import { DashboardStats } from '../../../core/models/dashboard.models';
import { selectCurrentUser } from '../../../core/store/auth/auth.selectors';

Chart.register(...registerables);

@Component({
  selector: 'app-admin-dashboard',
  standalone: true,
  imports: [CommonModule, MatIconModule, MatButtonModule],
  templateUrl: './admin-dashboard.component.html',
  styleUrls: ['./admin-dashboard.component.scss']
})
export class AdminDashboardComponent implements OnInit, AfterViewInit {

  @ViewChild('attendanceChart') attendanceChartRef!: ElementRef;
  @ViewChild('feeChart')        feeChartRef!: ElementRef;
  @ViewChild('marksChart')      marksChartRef!: ElementRef;

  private dashboardService = inject(DashboardService);
  private router           = inject(Router);
  private store            = inject(Store);

  user$         = this.store.select(selectCurrentUser);
  stats: DashboardStats | null = null;
  isLoading     = true;

  attendanceChart?: Chart;
  feeChart?: Chart;
  marksChart?: Chart;

  // KPI cards — updated with real data in ngOnInit
  kpiCards = [
    {
      title:      'Total Students',
      value:      '—',
      subtitle:   'Enrolled students',
      icon:       'school',
      trend:      0,
      trendLabel: 'real-time',
      color:      '#2563EB',
      lightColor: '#DBEAFE',
      route:      '/admin/users'
    },
    {
      title:      'Total Teachers',
      value:      '—',
      subtitle:   'Active staff',
      icon:       'person',
      trend:      0,
      trendLabel: 'real-time',
      color:      '#16A34A',
      lightColor: '#DCFCE7',
      route:      '/admin/users'
    },
    {
      title:      'Attendance Today',
      value:      '—',
      subtitle:   'Marked today',
      icon:       'fact_check',
      trend:      0,
      trendLabel: 'today',
      color:      '#D97706',
      lightColor: '#FEF3C7',
      route:      '/admin/attendance'
    },
    {
      title:      'Pending Leaves',
      value:      '—',
      subtitle:   'Awaiting approval',
      icon:       'event_busy',
      trend:      0,
      trendLabel: 'pending',
      color:      '#DC2626',
      lightColor: '#FEE2E2',
      route:      '/admin/leave-approvals'
    }
  ];

  recentActivities: any[] = [];

  quickActions = [
    { label: 'Post Notice',    icon: 'campaign',     color: '#2563EB', route: '/admin/notices'        },
    { label: 'Schedule Exam',  icon: 'event_note',   color: '#16A34A', route: '/admin/exams'          },
    { label: 'Add Teacher',    icon: 'person_add',   color: '#7C3AED', route: '/admin/users'          },
    { label: 'Enroll Student', icon: 'school',       color: '#D97706', route: '/admin/users'          },
    { label: 'Attendance',     icon: 'fact_check',   color: '#0D9488', route: '/admin/attendance'     },
    { label: 'Leave Approval', icon: 'event_available', color: '#DC2626', route: '/admin/leave-approvals' },
  ];

  ngOnInit(): void {
    this.dashboardService.getPrincipalStats().subscribe({
      next: (data) => {
        this.stats = data;
        this.isLoading = false;

        // Update KPI cards with real data
        this.kpiCards[0].value    = data.totalStudents.toString();
        this.kpiCards[1].value    = data.totalTeachers.toString();
        this.kpiCards[2].value    = `${data.attendanceTodayPercent}%`;
        this.kpiCards[3].value    = data.pendingLeaveApplications.toString();

        // Map recent activities
        this.recentActivities = data.recentActivities.map(a => ({
          user:   a.message.split(' ')[0],
          action: a.message,
          time:   this.getTimeAgo(a.occurredAt),
          icon:   a.type === 'Leave' ? 'event_busy' : 'campaign',
          color:  a.type === 'Leave' ? '#DC2626' : '#2563EB'
        }));

        setTimeout(() => {
          this.buildAttendanceChart(data.attendanceTodayPercent);
          this.buildFeeChart();
          this.buildMarksChart();
        }, 200);
      },
      error: () => {
        this.isLoading = false;
        setTimeout(() => {
          this.buildAttendanceChart(0);
          this.buildFeeChart();
          this.buildMarksChart();
        }, 200);
      }
    });
  }

  ngAfterViewInit(): void {}

  private buildAttendanceChart(presentPercent: number): void {
    const ctx = this.attendanceChartRef?.nativeElement?.getContext('2d');
    if (!ctx) return;

    if (this.attendanceChart) this.attendanceChart.destroy();

    this.attendanceChart = new Chart(ctx, {
      type: 'doughnut',
      data: {
        labels: ['Present', 'Absent', 'Late', 'On Leave'],
        datasets: [{
          data: [
            presentPercent > 0 ? presentPercent - 5 : 72,
            presentPercent > 0 ? 100 - presentPercent - 8 : 15,
            8, 5
          ],
          backgroundColor: ['#16A34A', '#DC2626', '#D97706', '#64748B'],
          borderWidth: 0,
          hoverOffset: 8
        }]
      },
      options: {
        responsive: true,
        maintainAspectRatio: false,
        cutout: '72%',
        plugins: {
          legend: {
            position: 'right',
            labels: { padding: 16, font: { size: 12, family: 'Inter' }, color: '#374151' }
          },
          tooltip: {
            callbacks: { label: c => ` ${c.label}: ${c.parsed}%` }
          }
        }
      }
    });
  }

  private buildFeeChart(): void {
    const ctx = this.feeChartRef?.nativeElement?.getContext('2d');
    if (!ctx) return;

    if (this.feeChart) this.feeChart.destroy();

    this.feeChart = new Chart(ctx, {
      type: 'bar',
      data: {
        labels: ['Jun','Jul','Aug','Sep','Oct','Nov','Dec','Jan','Feb','Mar','Apr','May'],
        datasets: [
          {
            label: 'Collected',
            data: [42000,58000,51000,67000,72000,55000,61000,84000,78000,92000,69000,54000],
            backgroundColor: '#2563EB',
            borderRadius: 6,
            barThickness: 14
          },
          {
            label: 'Pending',
            data: [8000,12000,9000,11000,8000,15000,10000,12000,14000,8000,11000,16000],
            backgroundColor: '#BFDBFE',
            borderRadius: 6,
            barThickness: 14
          }
        ]
      },
      options: {
        responsive: true,
        maintainAspectRatio: false,
        plugins: {
          legend: {
            position: 'top',
            labels: { font: { size: 12, family: 'Inter' }, color: '#374151', padding: 16 }
          },
          tooltip: {
            callbacks: { label: c => ` ${c.dataset.label}: ₹${((c.parsed.y ?? 0) / 1000).toFixed(0)}K` }
          }
        },
        scales: {
          x: { grid: { display: false }, ticks: { font: { size: 11, family: 'Inter' }, color: '#94A3B8' } },
          y: { grid: { color: '#F1F5F9' }, ticks: { font: { size: 11, family: 'Inter' }, color: '#94A3B8', callback: v => `₹${(+v/1000).toFixed(0)}K` } }
        }
      }
    });
  }

  private buildMarksChart(): void {
    const ctx = this.marksChartRef?.nativeElement?.getContext('2d');
    if (!ctx) return;

    if (this.marksChart) this.marksChart.destroy();

    this.marksChart = new Chart(ctx, {
      type: 'line',
      data: {
        labels: ['Unit Test','Mid Term','Final Exam'],
        datasets: [
          { label: 'Class 10A', data: [72, 76, 81], borderColor: '#2563EB', backgroundColor: 'rgba(37,99,235,0.08)', tension: 0.4, fill: true, pointBackgroundColor: '#2563EB', pointRadius: 5 },
          { label: 'Class 10B', data: [68, 73, 78], borderColor: '#16A34A', backgroundColor: 'rgba(22,163,74,0.08)', tension: 0.4, fill: true, pointBackgroundColor: '#16A34A', pointRadius: 5 },
          { label: 'Class 9A',  data: [65, 71, 75], borderColor: '#D97706', backgroundColor: 'rgba(217,119,6,0.08)',  tension: 0.4, fill: true, pointBackgroundColor: '#D97706', pointRadius: 5 }
        ]
      },
      options: {
        responsive: true,
        maintainAspectRatio: false,
        plugins: {
          legend: { position: 'top', labels: { font: { size: 12, family: 'Inter' }, color: '#374151', padding: 16 } }
        },
        scales: {
          x: { grid: { display: false }, ticks: { font: { size: 12, family: 'Inter' }, color: '#94A3B8' } },
          y: { min: 50, max: 100, grid: { color: '#F1F5F9' }, ticks: { font: { size: 11, family: 'Inter' }, color: '#94A3B8', callback: v => `${v}%` } }
        }
      }
    });
  }

  navigateTo(route: string): void { this.router.navigate([route]); }

  getTimeAgo(dateStr: string): string {
    const diff = Date.now() - new Date(dateStr).getTime();
    const hours = Math.floor(diff / 3600000);
    if (hours < 1) return 'Just now';
    if (hours < 24) return `${hours}h ago`;
    return `${Math.floor(hours / 24)}d ago`;
  }
}