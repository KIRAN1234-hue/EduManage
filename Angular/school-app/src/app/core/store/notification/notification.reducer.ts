import { createReducer, on } from '@ngrx/store';
import { AppNotification } from '../../models/notification.models';

// Named imports — avoids TS2306
import {
  loadNotifications,
  loadNotificationsSuccess,
  loadNotificationsFailure,
  markAsRead,
  markAllAsRead,
  deleteNotification,
  setUnreadCount
} from './notification.actions';

export interface NotificationState {
  notifications: AppNotification[];
  unreadCount:   number;
  isLoading:     boolean;
}

const initialState: NotificationState = {
  notifications: [],
  unreadCount:   0,
  isLoading:     false
};

export const notificationReducer = createReducer(
  initialState,

  on(loadNotifications, state => ({
    ...state, isLoading: true
  })),

  on(loadNotificationsSuccess,
    (state, { notifications }) => ({
      ...state,
      notifications,
      unreadCount: notifications.filter(
        (n: AppNotification) => !n.isRead   // explicit type fixes TS7006
      ).length,
      isLoading: false
    })
  ),

  on(loadNotificationsFailure, state => ({
    ...state, isLoading: false
  })),

  on(markAsRead, (state, { id }) => ({
    ...state,
    notifications: state.notifications.map(
      (n: AppNotification) => n.id === id ? { ...n, isRead: true } : n
    ),
    unreadCount: Math.max(0, state.unreadCount - 1)
  })),

  on(markAllAsRead, state => ({
    ...state,
    notifications: state.notifications.map(
      (n: AppNotification) => ({ ...n, isRead: true })
    ),
    unreadCount: 0
  })),

  on(deleteNotification, (state, { id }) => {
    const isUnread = state.notifications.find(
      (n: AppNotification) => n.id === id && !n.isRead
    );
    return {
      ...state,
      notifications: state.notifications.filter(
        (n: AppNotification) => n.id !== id
      ),
      unreadCount: isUnread
        ? Math.max(0, state.unreadCount - 1)
        : state.unreadCount
    };
  }),

  on(setUnreadCount, (state, { count }) => ({
    ...state, unreadCount: count
  }))
);