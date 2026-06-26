import { Component, OnInit, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { MatIconModule } from '@angular/material/icon';
import { MatButtonModule } from '@angular/material/button';
import { AttendancePercentage } from '../../../core/models/attendance.models';

@Component({
  selector: 'app-parent-attendance',
  standalone: true,
  imports: [CommonModule, MatIconModule, MatButtonModule],
  templateUrl: './parent-attendance.component.html',
  styleUrls: ['./parent-attendance.component.scss']
})
export class ParentAttendanceComponent implements OnInit {

  // Mock — Week 5 will get real child's studentId
  childName = 'Rahul Sharma';
  childClass = 'Class 10A';

  subjectPercentages: AttendancePercentage[] = [
    { subjectId:'1', subjectName:'Mathematics', totalClasses:40, presentCount:36, absentCount:2, lateCount:2, percentage:95.0, riskLevel:'Green' },
    { subjectId:'2', subjectName:'Science',     totalClasses:38, presentCount:30, absentCount:5, lateCount:3, percentage:86.8, riskLevel:'Green' },
    { subjectId:'3', subjectName:'English',     totalClasses:42, presentCount:32, absentCount:7, lateCount:3, percentage:83.3, riskLevel:'Green' },
    { subjectId:'6', subjectName:'Computer',    totalClasses:25, presentCount:16, absentCount:7, lateCount:2, percentage:72.0, riskLevel:'Amber' },
    { subjectId:'8', subjectName:'Chemistry',   totalClasses:28, presentCount:16, absentCount:10,lateCount:2, percentage:64.3, riskLevel:'Red'   },
  ];

  overallPercentage = 0;

  ngOnInit(): void {
    const totalClasses  = this.subjectPercentages.reduce((s,x) => s + x.totalClasses, 0);
    const totalAttended = this.subjectPercentages.reduce((s,x) => s + x.presentCount + x.lateCount, 0);
    this.overallPercentage = totalClasses > 0
      ? Math.round(totalAttended / totalClasses * 100) : 0;
  }

  getRiskConfig(risk: string): { color: string; bg: string } {
    const map: Record<string, any> = {
      Green: { color: '#16A34A', bg: '#DCFCE7' },
      Amber: { color: '#D97706', bg: '#FEF3C7' },
      Red:   { color: '#DC2626', bg: '#FEE2E2' }
    };
    return map[risk] ?? map['Green'];
  }
}