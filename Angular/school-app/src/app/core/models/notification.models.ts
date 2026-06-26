export interface AppNotification {
  id: string;
  title: string;
  body: string;
  notificationType: string;
  isRead: boolean;
  createdAt: string;
  actionUrl?: string;
  relatedEntityId?: string;
}