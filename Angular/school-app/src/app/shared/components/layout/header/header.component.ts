import {
  Component, OnInit, inject,
  Output, EventEmitter
} from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router } from '@angular/router';
import { Store } from '@ngrx/store';
import { MatIconModule } from '@angular/material/icon';
import { MatMenuModule } from '@angular/material/menu';
import { MatButtonModule } from '@angular/material/button';

// ✅ FIXED: 4 levels up (../../../../) not 3 (../../../)
import {
  selectAllNotifications,
  selectUnreadCount
} from '../../../../core/store/notification/notification.selectors';

import {
  loadNotifications,
  markAsRead as markAsReadAction,
  markAllAsRead as markAllAsReadAction,
  deleteNotification
} from '../../../../core/store/notification/notification.actions';

import { logout } from '../../../../core/store/auth/auth.actions';

import {
  selectCurrentUser,
  selectUserRole
} from '../../../../core/store/auth/auth.selectors';

import { AppNotification } from '../../../../core/models/notification.models';

@Component({
  selector: 'app-header',
  standalone: true,
  imports: [CommonModule, MatIconModule, MatMenuModule, MatButtonModule],
  templateUrl: './header.component.html',
  styleUrls: ['./header.component.scss']
})
export class HeaderComponent implements OnInit {
  private store = inject(Store);
  router        = inject(Router);

  @Output() menuToggle = new EventEmitter<void>();

  user$          = this.store.select(selectCurrentUser);
  role$          = this.store.select(selectUserRole);
  notifications$ = this.store.select(selectAllNotifications);
  unreadCount$   = this.store.select(selectUnreadCount);

  showNotifPanel = false;

  get pageTitle(): string {
    const segments = this.router.url.split('/').filter(s => s);
    if (segments.length < 2) return 'Dashboard';
    const last = segments[segments.length - 1];
    return last.charAt(0).toUpperCase() +
      last.slice(1).replace(/-/g, ' ');
  }

  ngOnInit(): void {
    this.store.dispatch(loadNotifications());
  }

  onMenuToggle(): void {
    this.menuToggle.emit();
  }

  togglePanel(): void {
    this.showNotifPanel = !this.showNotifPanel;
    if (this.showNotifPanel) {
      this.store.dispatch(loadNotifications());
    }
  }

  markAsRead(notif: AppNotification): void {
    if (!notif.isRead) {
      this.store.dispatch(markAsReadAction({ id: notif.id }));
    }
    if (notif.actionUrl) {
      this.router.navigate([notif.actionUrl]);
      this.showNotifPanel = false;
    }
  }

  markAllRead(): void {
    this.store.dispatch(markAllAsReadAction());
  }

  deleteNotif(id: string, event: Event): void {
    event.stopPropagation();
    this.store.dispatch(deleteNotification({ id }));
  }

  onLogout(): void {
    this.store.dispatch(logout());
  }

  getNotifIcon(type: string): string {
    const map: Record<string, string> = {
      Assignment: 'assignment', Leave: 'event_busy',
      Mark: 'grade', Notice: 'campaign',
      Message: 'mail', Fee: 'payments', General: 'notifications'
    };
    return map[type] ?? 'notifications';
  }

  getNotifColor(type: string): string {
    const map: Record<string, string> = {
      Assignment: '#7C3AED', Leave: '#DC2626',
      Mark: '#16A34A', Notice: '#2563EB',
      Message: '#D97706', Fee: '#0891B2', General: '#64748B'
    };
    return map[type] ?? '#64748B';
  }

  getTimeAgo(date: string): string {
    const diff = Date.now() - new Date(date).getTime();
    const mins = Math.floor(diff / 60000);
    if (mins < 1) return 'Just now';
    if (mins < 60) return `${mins}m ago`;
    const hrs = Math.floor(mins / 60);
    if (hrs < 24) return `${hrs}h ago`;
    return `${Math.floor(hrs / 24)}d ago`;
  }
}