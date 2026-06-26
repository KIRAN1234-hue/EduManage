export interface MessageResponse {
  id: string;
  senderName: string;
  receiverName: string;
  senderId: string;
  receiverId: string;
  subject: string;
  content: string;
  messageType: string;
  sentAt: string;
  isRead: boolean;
  parentMessageId?: string;
  replyCount: number;
  replies?: MessageResponse[];
}

export interface SendMessageRequest {
  receiverId: string;
  subject: string;
  content: string;
  messageType: string;
  parentMessageId?: string;
}

export interface ConversationSummary {
  otherUserId: string;
  otherUserName: string;
  otherUserRole: string;
  lastMessage: string;
  lastMessageAt: string;
  unreadCount: number;
}