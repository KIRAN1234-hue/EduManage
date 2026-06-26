export interface TimetableResponse {
  id: string;
  className: string;
  subjectName: string;
  teacherName: string;
  dayOfWeek: string;
  periodNumber: number;
  startTime: string;
  endTime: string;
  academicYear: string;
}

export interface CreateTimetableRequest {
  classId: string;
  subjectId: string;
  teacherId: string;
  dayOfWeek: number;
  periodNumber: number;
  startTime: string;
  endTime: string;
  academicYear: string;
}