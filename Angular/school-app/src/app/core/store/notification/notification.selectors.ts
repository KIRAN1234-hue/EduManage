import { createFeatureSelector, createSelector } from '@ngrx/store';
import { NotificationState } from './notification.reducer';

export const selectNotificationState =
  createFeatureSelector<NotificationState>('notifications');

export const selectAllNotifications = createSelector(
  selectNotificationState,
  state => state.notifications
);

export const selectUnreadCount = createSelector(
  selectNotificationState,
  state => state.unreadCount
);

export const selectIsLoadingNotifications = createSelector(
  selectNotificationState,
  state => state.isLoading
);