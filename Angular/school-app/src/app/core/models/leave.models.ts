export interface LeaveResponse {
  id: string;
  studentName: string;
  rollNumber: string;
  leaveType: string;
  fromDate: string;
  toDate: string;
  totalDays: number;
  reason: string;
  remarks?: string;
  status: string;
  approvalRemark?: string;
  approvedBy?: string;
  appliedOn: string;
}

export interface CreateLeaveRequest {
  leaveType: string;
  fromDate: string;
  toDate: string;
  reason: string;
  remarks?: string;
}

export interface ApproveLeaveRequest {
  isApproved: boolean;
  approvalRemark?: string;
}