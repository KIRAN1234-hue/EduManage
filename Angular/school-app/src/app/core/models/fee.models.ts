export interface FeeStructureResponse {
  id: string;
  className: string;
  academicYear: string;
  termName: string;
  amount: number;
  dueDate: string;
  discountAllowed: boolean;
  description: string;
  paidCount: number;
  totalCollected: number;
  isOverdue: boolean;
}

export interface CreateFeeStructureRequest {
  classId: string;
  academicYear: string;
  termName: string;
  amount: number;
  dueDate: string;
  discountAllowed: boolean;
  description: string;
}

export interface RecordPaymentRequest {
  studentId: string;
  feeStructureId: string;
  amount: number;
  discountAmount: number;
  paymentMethod: string;
  remarks?: string;
  receiptUrl?: string;
}

export interface FeePaymentResponse {
  id: string;
  studentName: string;
  rollNumber: string;
  termName: string;
  amount: number;
  discountAmount: number;
  netAmount: number;
  paymentDate: string;
  receiptNumber: string;
  receiptUrl?: string;
  status: string;
  paymentMethod: string;
  remarks?: string;
  recordedBy: string;
  totalFee: number;
  balance: number;
}

export interface StudentFeeStatus {
  studentName: string;
  rollNumber: string;
  className: string;
  terms: FeeTermStatus[];
  totalDue: number;
  totalPaid: number;
  balance: number;
}

export interface FeeTermStatus {
  feeStructureId: string;
  termName: string;
  amount: number;
  paid: number;
  status: string;
  dueDate: string;
  receiptNumber?: string;
}

export interface ParentPaymentRequest {
  feeStructureId: string;
  amount: number;
  paymentMethod: string;
  remarks?: string;
}