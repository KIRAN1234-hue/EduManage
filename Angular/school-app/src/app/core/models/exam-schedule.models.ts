export interface ExamScheduleResponse {
  id: string;
  subjectName: string;
  className: string;
  invigilatorTeacher: string;
  examType: string;
  examDate: string;
  startTime: string;
  endTime: string;
  roomNumber: string;
  marksEntryOpen: boolean;
}

export interface CreateExamScheduleRequest {
  subjectId: string;
  classId: string;
  invigilatorTeacherId: string;
  examType: number;
  examDate: string;
  startTime: string;
  endTime: string;
  roomNumber: string;
}