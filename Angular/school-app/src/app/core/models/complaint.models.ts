export interface ComplaintResponse {
  id: string;
  title: string;
  category: string;
  description: string;
  status: string;
  submittedBy: string;
  assignedTo?: string;
  resolutionRemark?: string;
  createdAt: string;
  resolvedAt?: string;
}

export interface CreateComplaintRequest {
  title: string;
  category: number;
  description: string;
}

export interface UpdateComplaintRequest {
  assignedToUserId?: string;
  newStatus: string;
  resolutionRemark?: string;
}