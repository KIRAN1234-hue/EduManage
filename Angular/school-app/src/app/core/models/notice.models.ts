export interface NoticeResponse {
  id: string;
  title: string;
  content: string;
  targetRole: string;
  attachmentUrl?: string;
  isArchived: boolean;
  postedBy: string;
  publishedAt: string;
  isAcknowledgedByMe: boolean;
  acknowledgementCount: number;
}

export interface CreateNoticeRequest {
  title: string;
  content: string;
  targetRole: number;
  attachmentUrl?: string;
}

// NoticeTargetRole enum values — must match C# enum
export enum NoticeTargetRole {
  All       = 0,
  Student   = 1,
  Teacher   = 2,
  Parent    = 3,
  Principal = 4
}