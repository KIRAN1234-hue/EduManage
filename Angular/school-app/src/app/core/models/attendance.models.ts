export interface MarkAttendanceRequest {
  studentId: string;
  subjectId: string;
  date: string;
  status: 'Present' | 'Absent' | 'Late' | 'OnLeave';
  remarks?: string;
}

export interface AttendanceResponse {
  id: string;
  studentId: string;
  studentName: string;
  rollNumber: string;
  subjectId: string;
  subjectName: string;
  date: string;
  status: string;
  remarks?: string;
  markedByTeacher: string;
}

export interface AttendancePercentage {
  subjectId: string;
  subjectName: string;
  totalClasses: number;
  presentCount: number;
  absentCount: number;
  lateCount: number;
  percentage: number;
  riskLevel: 'Green' | 'Amber' | 'Red';
}

export interface StudentAttendanceRow {
  studentId: string;
  studentName: string;
  rollNumber: string;
  status: 'Present' | 'Absent' | 'Late' | 'OnLeave';
  remarks: string;
}

// For class and subject dropdowns
export interface ClassItem {
  id: string;
  name: string;
  section: string;
  academicYear: string;
}

export interface SubjectItem {
  id: string;
  name: string;
  code: string;
}