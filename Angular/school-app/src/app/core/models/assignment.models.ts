export interface AssignmentResponse {
  id: string;
  title: string;
  description: string;
  dueDate: string;
  totalMarks: number;
  filePath?: string;
  isActive: boolean;
  subjectName: string;
  className: string;
  teacherName: string;
  createdAt: string;
  submissionCount: number;
  isOverdue: boolean;
}

export interface CreateAssignmentRequest {
  title: string;
  description: string;
  dueDate: string;
  subjectId: string;
  classId: string;
  totalMarks: number;
}

export interface SubmitAssignmentRequest {
  content: string;
  filePath?: string;
}

export interface SubmissionResponse {
  id: string;
  studentName: string;
  rollNumber: string;
  content: string;
  filePath?: string;
  submittedAt: string;
  isLate: boolean;
  marksAwarded?: number;
  teacherRemark?: string;
  feedback?: string;
  status: string;
  assignmentTitle: string;
}

export interface GradeSubmissionRequest {
  marksAwarded: number;
  teacherRemark?: string;
  feedback?: string;
}