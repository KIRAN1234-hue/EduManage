import { Injectable, inject } from '@angular/core';
import { Actions, createEffect, ofType } from '@ngrx/effects';
import { timer } from 'rxjs';
import { switchMap, map, catchError, exhaustMap } from 'rxjs/operators';
import { of } from 'rxjs';

import { NotificationService } from '../../services/notification.service';

// Named imports — avoids TS2306
import {
  loadNotifications,
  loadNotificationsSuccess,
  loadNotificationsFailure,
  markAsRead,
  markAllAsRead,
  deleteNotification,
  refreshUnreadCount,
  setUnreadCount
} from './notification.actions';

@Injectable()
export class NotificationEffects {
  private actions$           = inject(Actions);
  private notificationService = inject(NotificationService);

  loadNotifications$ = createEffect(() =>
    this.actions$.pipe(
      ofType(loadNotifications),
      switchMap(() =>
        this.notificationService.getMyNotifications().pipe(
          map(notifications =>
            loadNotificationsSuccess({ notifications })
          ),
          catchError(() => of(loadNotificationsFailure()))
        )
      )
    )
  );

  markAsRead$ = createEffect(() =>
    this.actions$.pipe(
      ofType(markAsRead),
      exhaustMap(({ id }) =>
        this.notificationService.markAsRead(id).pipe(
          map(() => loadNotifications()),
          catchError(() => of(loadNotificationsFailure()))
        )
      )
    )
  );

  markAllAsRead$ = createEffect(() =>
    this.actions$.pipe(
      ofType(markAllAsRead),
      exhaustMap(() =>
        this.notificationService.markAllAsRead().pipe(
          map(() => loadNotifications()),
          catchError(() => of(loadNotificationsFailure()))
        )
      )
    )
  );

  deleteNotification$ = createEffect(() =>
    this.actions$.pipe(
      ofType(deleteNotification),
      exhaustMap(({ id }) =>
        this.notificationService.delete(id).pipe(
          map(() => loadNotifications()),
          catchError(() => of(loadNotificationsFailure()))
        )
      )
    )
  );

  autoRefresh$ = createEffect(() =>
    timer(5000, 30000).pipe(
      map(() => refreshUnreadCount())
    )
  );

  refreshUnreadCount$ = createEffect(() =>
    this.actions$.pipe(
      ofType(refreshUnreadCount),
      switchMap(() =>
        this.notificationService.getUnreadCount().pipe(
          map(({ count }) => setUnreadCount({ count })),
          catchError(() => of(loadNotificationsFailure()))
        )
      )
    )
  );
}