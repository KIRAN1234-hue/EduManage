export interface CreateMarkRequest {
  studentId: string;
  subjectId: string;
  examType: string;
  marksObtained: number;
  maxMarks: number;
}

export interface MarkResponse {
  id: string;
  studentId: string;
  studentName: string;
  rollNumber: string;
  subjectId: string;
  subjectName: string;
  examType: string;
  marksObtained: number;
  maxMarks: number;
  percentage: number;
  grade: string;
  enteredAt: string;
  enteredByTeacher: string;
}

export interface ReportCard {
  studentId: string;
  studentName: string;
  rollNumber: string;
  className: string;
  academicYear: string;
  overallPercentage: number;
  overallGrade: string;
  subjects: SubjectReport[];
}

export interface SubjectReport {
  subjectName: string;
  subjectCode: string;
  exams: ExamMark[];
  subjectPercentage: number;
  subjectGrade: string;
}

export interface ExamMark {
  examType: string;
  marksObtained: number;
  maxMarks: number;
  grade: string;
}

export interface MarksChartData {
  labels: string[];
  datasets: MarksChartDataset[];
}

export interface MarksChartDataset {
  label: string;
  data: number[];
  backgroundColor: string;
}