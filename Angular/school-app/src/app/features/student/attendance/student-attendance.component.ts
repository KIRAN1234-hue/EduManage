import {
  Component, OnInit, AfterViewInit,
  inject, ViewChild, ElementRef, signal
} from '@angular/core';
import { CommonModule } from '@angular/common';
import { MatIconModule } from '@angular/material/icon';
import { MatButtonModule } from '@angular/material/button';
import { AttendanceService } from '../../../core/services/attendance.service';
import { AttendancePercentage, AttendanceResponse } from '../../../core/models/attendance.models';
import { Chart, registerables } from 'chart.js';

Chart.register(...registerables);

interface CalendarDay {
  date: number;
  status: 'Present' | 'Absent' | 'Late' | 'OnLeave' | 'Weekend' | 'Future' | 'Empty';
}

@Component({
  selector: 'app-student-attendance',
  standalone: true,
  imports: [CommonModule, MatIconModule, MatButtonModule],
  templateUrl: './student-attendance.component.html',
  styleUrls: ['./student-attendance.component.scss']
})
export class StudentAttendanceComponent implements OnInit, AfterViewInit {
  @ViewChild('trendChart') trendChartRef!: ElementRef;

  private attendanceService = inject(AttendanceService);

  isLoading         = signal(true);
  trendChart?: Chart;

  subjectPercentages: AttendancePercentage[] = [];
  attendanceRecords:  AttendanceResponse[]   = [];

  calendarDays: CalendarDay[] = [];
  currentMonth = new Date();

  overallPercentage = 0;
  totalPresent      = 0;
  totalAbsent       = 0;
  totalLate         = 0;
  atRiskSubjects    = 0;

  ngOnInit(): void {
    // Load subject percentages — real API
    this.attendanceService.getMyAttendancePercentage().subscribe({
      next: (data) => {
        this.subjectPercentages = data;
        this.calculateSummary();
        this.isLoading.set(false);
        setTimeout(() => this.buildTrendChart(), 200);
      },
      error: () => this.isLoading.set(false)
    });

    // Load attendance records for calendar
    this.attendanceService.getMyAttendance().subscribe({
      next: (records) => {
        this.attendanceRecords = records;
        this.buildCalendar();
      }
    });
  }

  ngAfterViewInit(): void {}

  private calculateSummary(): void {
    const total = this.subjectPercentages
      .reduce((s, x) => s + x.totalClasses, 0);
    this.totalPresent = this.subjectPercentages
      .reduce((s, x) => s + x.presentCount + x.lateCount, 0);
    this.totalAbsent = this.subjectPercentages
      .reduce((s, x) => s + x.absentCount, 0);
    this.totalLate = this.subjectPercentages
      .reduce((s, x) => s + x.lateCount, 0);
    this.atRiskSubjects = this.subjectPercentages
      .filter(s => s.riskLevel !== 'Green').length;
    this.overallPercentage = total > 0
      ? Math.round(this.totalPresent / total * 100) : 0;
  }

  private buildCalendar(): void {
    const year  = this.currentMonth.getFullYear();
    const month = this.currentMonth.getMonth();
    const first = new Date(year, month, 1).getDay();
    const days  = new Date(year, month + 1, 0).getDate();
    const today = new Date();

    this.calendarDays = [];

    // Empty leading cells
    for (let i = 0; i < first; i++) {
      this.calendarDays.push({ date: 0, status: 'Empty' });
    }

    for (let d = 1; d <= days; d++) {
      const thisDate   = new Date(year, month, d);
      const dayOfWeek  = thisDate.getDay();
      const dateStr    = `${year}-${String(month + 1).padStart(2, '0')}-${String(d).padStart(2, '0')}`;

      let status: CalendarDay['status'] = 'Future';

      if (thisDate > today) {
        status = 'Future';
      } else if (dayOfWeek === 0 || dayOfWeek === 6) {
        status = 'Weekend';
      } else {
        // Find matching record from real API data
        const record = this.attendanceRecords.find(r =>
          r.date?.startsWith(dateStr)
        );
        if (record) {
          status = record.status as any;
        } else {
          status = 'Future'; // No record = not marked yet
        }
      }

      this.calendarDays.push({ date: d, status });
    }
  }

  private buildTrendChart(): void {
    const ctx = this.trendChartRef?.nativeElement?.getContext('2d');
    if (!ctx) return;

    // Build 30-day trend from real records
    const labels: string[] = [];
    const data: number[]   = [];

    for (let i = 29; i >= 0; i--) {
      const d = new Date();
      d.setDate(d.getDate() - i);
      const label   = `${d.getDate()}/${d.getMonth() + 1}`;
      const dateStr = d.toISOString().split('T')[0];

      labels.push(label);

      // Calculate daily % from records
      const dayRecords = this.attendanceRecords.filter(r =>
        r.date?.startsWith(dateStr)
      );

      if (dayRecords.length === 0) {
        data.push(this.overallPercentage || 0);
      } else {
        const present = dayRecords.filter(
          r => r.status === 'Present' || r.status === 'Late'
        ).length;
        data.push(Math.round(present / dayRecords.length * 100));
      }
    }

    this.trendChart = new Chart(ctx, {
      type: 'line',
      data: {
        labels,
        datasets: [{
          label: 'Daily Attendance %',
          data,
          borderColor: '#2563EB',
          backgroundColor: 'rgba(37,99,235,0.08)',
          tension: 0.4,
          fill: true,
          pointRadius: 0,
          pointHoverRadius: 5,
          borderWidth: 2
        }]
      },
      options: {
        responsive: true,
        maintainAspectRatio: false,
        plugins: {
          legend: { display: false },
          tooltip: { callbacks: { label: c => ` ${c.parsed.y}%` } }
        },
        scales: {
          x: {
            grid: { display: false },
            ticks: { maxTicksLimit: 8, font: { size: 10 }, color: '#94A3B8' }
          },
          y: {
            min: 0, max: 100,
            grid: { color: '#F1F5F9' },
            ticks: { font: { size: 10 }, color: '#94A3B8', callback: v => `${v}%` }
          }
        }
      }
    });
  }

  getRiskConfig(risk: string): { color: string; bg: string; icon: string } {
    const map: Record<string, any> = {
      Green: { color: '#16A34A', bg: '#DCFCE7', icon: 'check_circle' },
      Amber: { color: '#D97706', bg: '#FEF3C7', icon: 'warning'      },
      Red:   { color: '#DC2626', bg: '#FEE2E2', icon: 'error'        }
    };
    return map[risk] ?? map['Green'];
  }

  getMonthName(): string {
    return this.currentMonth.toLocaleDateString('en-US', {
      month: 'long', year: 'numeric'
    });
  }

  prevMonth(): void {
    this.currentMonth = new Date(
      this.currentMonth.getFullYear(),
      this.currentMonth.getMonth() - 1, 1
    );
    this.buildCalendar();
  }

  nextMonth(): void {
    const next = new Date(
      this.currentMonth.getFullYear(),
      this.currentMonth.getMonth() + 1, 1
    );
    if (next <= new Date()) {
      this.currentMonth = next;
      this.buildCalendar();
    }
  }
}