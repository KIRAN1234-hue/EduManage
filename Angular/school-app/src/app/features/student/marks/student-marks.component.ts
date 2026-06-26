import {
  Component, OnInit,
  inject, ViewChild, ElementRef, signal
} from '@angular/core';
import { CommonModule } from '@angular/common';
import { MatIconModule } from '@angular/material/icon';
import { MatButtonModule } from '@angular/material/button';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { Chart, registerables } from 'chart.js';
import { MarksService } from '../../../core/services/marks.service';
import { ReportCard, SubjectReport } from '../../../core/models/marks.models';

Chart.register(...registerables);

@Component({
  selector: 'app-student-marks',
  standalone: true,
  imports: [
    CommonModule, MatIconModule,
    MatButtonModule, MatProgressSpinnerModule
  ],
  templateUrl: './student-marks.component.html',
  styleUrls: ['./student-marks.component.scss']
})
export class StudentMarksComponent implements OnInit {
  @ViewChild('performanceChart') chartRef!: ElementRef;

  private marksService = inject(MarksService);

  isLoading  = signal(true);
  hasData    = signal(false);
  chart?: Chart;

  reportCard: ReportCard | null = null;

  ngOnInit(): void {
  this.marksService.getMyReportCard().subscribe({
    next: (data) => {
      this.reportCard = data;
      this.hasData.set(true);
      this.isLoading.set(false);

      // Wait until Angular creates the canvas
      setTimeout(() => this.buildChart(), 200);
    },
    error: () => {
      this.hasData.set(false);
      this.isLoading.set(false);
    }
  });
}

  

  private buildChart(): void {
  if (!this.reportCard || !this.chartRef) return;
  const ctx = this.chartRef.nativeElement?.getContext('2d');
  if (!ctx) return;

  if (this.chart) this.chart.destroy();

  const subjects  = this.reportCard.subjects.map(s => s.subjectName);
  const examTypes = this.allExamTypes;

  const palette = [
    { bg: 'rgba(99,102,241,0.82)',  border: '#6366F1' },  // indigo
    { bg: 'rgba(16,185,129,0.82)',  border: '#10B981' },  // emerald
    { bg: 'rgba(245,158,11,0.82)',  border: '#F59E0B' },  // amber
    { bg: 'rgba(239,68,68,0.82)',   border: '#EF4444' },  // red
  ];

  const labelMap: Record<string,string> = {
    UnitTest:'Unit Test', MidTerm:'Mid Term',
    Final:'Final', Practical:'Practical'
  };

  const datasets = examTypes.map((et, i) => ({
    label:           labelMap[et] ?? et,
    data: this.reportCard!.subjects.map(s => {
      const exam = s.exams.find(e => e.examType === et);
      return exam ? Math.round(exam.marksObtained / exam.maxMarks * 100) : 0;
    }),
    backgroundColor: palette[i % palette.length].bg,
    borderColor:     palette[i % palette.length].border,
    borderWidth:     0,
    borderRadius:    6,
    borderSkipped:   false,
    barThickness:    25,
  }));

  this.chart = new Chart(ctx, {
    type: 'bar',
    data: { labels: subjects, datasets },
    options: {
      responsive:          true,
      maintainAspectRatio: false,
      animation:           { duration: 700, easing: 'easeInOutQuart' },
      plugins: {
        legend: {
          position: 'top',
          align:    'center',
          labels: {
            padding:        20,
            usePointStyle:  true,
            pointStyle:     'circle',
            font:           { size: 12, family: 'Inter' },
            color:          '#374151',
            boxWidth:       8
          }
        },
        tooltip: {
          mode:         'index',
          intersect:    false,
          backgroundColor: 'rgba(15,23,42,0.88)',
          padding:      10,
          cornerRadius: 8,
          titleFont:    { size: 12, family: 'Inter', weight: 600 },
          bodyFont:     { size: 12, family: 'Inter' },
          callbacks: {
            label: c => `  ${c.dataset.label}: ${c.parsed.y}%`
          }
        }
      },
      scales: {
        x: {
          grid:   { display: false },
          border: { display: false },
          ticks: {
            font:  { size: 12, family: 'Inter' },
            color: '#64748B',
            padding: 6
          }
        },
        y: {
          min:  40,
          max:  100,
          grid: {
            color:     'rgba(0,0,0,0.045)',
            lineWidth: 1
          },
          border: { display: false, dash: [3, 3] },
          ticks: {
            count:    7,
            font:     { size: 11, family: 'Inter' },
            color:    '#94A3B8',
            padding:  8,
            callback: v => `${v}%`
          }
        }
      },
      layout: {
        padding: { left: 4, right: 4, top: 8, bottom: 4 }
      }
    }
  });
}

  getGradeColor(grade: string): string {
    const map: Record<string,string> = { 'A+':'#16A34A','A':'#22C55E','B':'#2563EB','C':'#D97706','D':'#EA580C','F':'#DC2626' };
    return map[grade] ?? '#94A3B8';
  }

  getGradeBg(grade: string): string {
    const map: Record<string,string> = { 'A+':'#DCFCE7','A':'#F0FDF4','B':'#DBEAFE','C':'#FEF3C7','D':'#FFEDD5','F':'#FEE2E2' };
    return map[grade] ?? '#F1F5F9';
  }

  getExamMark(subject: SubjectReport, examType: string) {
    return subject.exams.find(e => e.examType === examType);
  }

  get allExamTypes(): string[] {
    if (!this.reportCard) return [];
    const types = new Set<string>();
    this.reportCard.subjects.forEach(s => s.exams.forEach(e => types.add(e.examType)));
    return Array.from(types);
  }

  get examTypeLabels(): Record<string,string> {
    return { UnitTest:'Unit Test', MidTerm:'Mid Term', Final:'Final', Practical:'Practical' };
  }
}