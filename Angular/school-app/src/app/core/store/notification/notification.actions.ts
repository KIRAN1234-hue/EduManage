import { createAction, props } from '@ngrx/store';
import { AppNotification } from '../../models/notification.models';

export const loadNotifications = createAction(
  '[Notification] Load'
);

export const loadNotificationsSuccess = createAction(
  '[Notification] Load Success',
  props<{ notifications: AppNotification[] }>()
);

export const loadNotificationsFailure = createAction(
  '[Notification] Load Failure'
);

export const markAsRead = createAction(
  '[Notification] Mark As Read',
  props<{ id: string }>()
);

export const markAllAsRead = createAction(
  '[Notification] Mark All As Read'
);

export const deleteNotification = createAction(
  '[Notification] Delete',
  props<{ id: string }>()
);

export const refreshUnreadCount = createAction(
  '[Notification] Refresh Unread Count'
);

export const setUnreadCount = createAction(
  '[Notification] Set Unread Count',
  props<{ count: number }>()
);