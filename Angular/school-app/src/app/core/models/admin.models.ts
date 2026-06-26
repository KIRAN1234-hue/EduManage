export interface CreateClassRequest {
  name: string;
  section: string;
  academicYear: string;
  maxStrength: number;
}

export interface ClassResponse {
  id: string;
  name: string;
  section: string;
  academicYear: string;
  maxStrength: number;
  studentCount: number;
  classTeacher: string;
}

export interface CreateSubjectRequest {
  name: string;
  code: string;
  maxMarks: number;
  isElective: boolean;
  classId: string;
  teacherId?: string;
}

export interface SubjectResponse {
  id: string;
  name: string;
  code: string;
  maxMarks: number;
  isElective: boolean;
  className: string;
  teacherName: string;
}

export interface CreateTeacherRequest {
  fullName: string;
  email: string;
  qualification: string;
  employeeCode: string;
  joiningDate: string;
  classId?: string;
  isClassTeacher: boolean;
}

export interface TeacherResponse {
  id: string;
  fullName: string;
  email: string;
  employeeCode: string;
  qualification: string;
  className?: string;
  isActive: boolean;
  inviteToken?: string;
  userId: string;
}

export interface CreateStudentRequest {
  fullName: string;
  email: string;
  rollNumber: string;
  classId: string;
  dateOfBirth: string;
  admissionDate: string;
}

export interface StudentResponse {
  id: string;
  fullName: string;
  email: string;
  rollNumber: string;
  className: string;
  dateOfBirth: string;
  admissionDate: string;
  isActive: boolean;
  inviteToken?: string;
  userId: string; 
}

export interface RegisterTeacherRequest {
  email: string;
  inviteToken: string;
  newPassword: string;
}

export interface RegisterStudentRequest {
  rollNumber: string;
  dateOfBirth: string;
  email: string;
  newPassword: string;
}

export interface RegisterParentRequest {
  fullName: string;
  email: string;
  password: string;
  studentRollNumber: string;
  studentEmail: string;
  phoneNumber?: string;
}

export interface TeacherForDoubt {
  userId:      string;   // AspNetUsers.Id — used as receiverId in messages
  fullName:    string;
  subjectName: string;
}