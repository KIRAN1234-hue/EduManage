import {
  Component, OnInit, inject, signal
} from '@angular/core';
import { CommonModule } from '@angular/common';
import {
  ReactiveFormsModule, FormBuilder,
  FormGroup, Validators
} from '@angular/forms';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatSelectModule } from '@angular/material/select';
import { MatSnackBar, MatSnackBarModule } from '@angular/material/snack-bar';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { Store } from '@ngrx/store';

import { NoticeService } from '../../../core/services/notice.service';
import { NoticeResponse, NoticeTargetRole } from '../../../core/models/notice.models';
import { selectUserRole, selectCurrentUser } from '../../../core/store/auth/auth.selectors';

@Component({
  selector: 'app-notices',
  standalone: true,
  imports: [
    CommonModule, ReactiveFormsModule,
    MatButtonModule, MatIconModule,
    MatFormFieldModule, MatInputModule,
    MatSelectModule, MatSnackBarModule,
    MatProgressSpinnerModule
  ],
  templateUrl: './notices.component.html',
  styleUrls: ['./notices.component.scss']
})
export class NoticesComponent implements OnInit {
  private noticeService = inject(NoticeService);
  private snackBar      = inject(MatSnackBar);
  private store         = inject(Store);
  private fb            = inject(FormBuilder);

  role$    = this.store.select(selectUserRole);
  user$    = this.store.select(selectCurrentUser);
  userRole = '';

  notices:     NoticeResponse[] = [];
  isLoading    = signal(false);
  isCreating   = signal(false);
  showCreate   = signal(false);

  createForm!: FormGroup;

  targetRoles = [
    { value: NoticeTargetRole.All,       label: 'Everyone'     },
    { value: NoticeTargetRole.Student,   label: 'Students Only' },
    { value: NoticeTargetRole.Teacher,   label: 'Teachers Only' },
    { value: NoticeTargetRole.Parent,    label: 'Parents Only'  },
  ];

  ngOnInit(): void {
    this.createForm = this.fb.group({
      title:      ['', Validators.required],
      content:    ['', Validators.required],
      targetRole: [NoticeTargetRole.All, Validators.required]
    });

    this.role$.subscribe(r => this.userRole = r ?? '');
    this.loadNotices();
  }

  canCreate(): boolean {
    return ['principal', 'teacher'].includes(this.userRole.toLowerCase());
  }

  canDelete(): boolean {
    return this.userRole.toLowerCase() === 'principal';
  }

  loadNotices(): void {
    this.isLoading.set(true);
    this.noticeService.getNotices().subscribe({
      next: data => {
        this.notices = data;
        this.isLoading.set(false);
      },
      error: () => this.isLoading.set(false)
    });
  }

  createNotice(): void {
    if (this.createForm.invalid) return;
    this.isCreating.set(true);

    this.noticeService.createNotice(this.createForm.value).subscribe({
      next: (res) => {
        this.notices.unshift(res);
        this.createForm.reset({ targetRole: NoticeTargetRole.All });
        this.showCreate.set(false);
        this.snackBar.open('Notice posted!', 'Close', {
          duration: 3000, panelClass: ['success-snackbar']
        });
        this.isCreating.set(false);
      },
      error: (err) => {
        this.snackBar.open(err.error?.message ?? 'Failed to post notice.', 'Close', {
          duration: 4000, panelClass: ['error-snackbar']
        });
        this.isCreating.set(false);
      }
    });
  }

  acknowledge(notice: NoticeResponse): void {
    if (notice.isAcknowledgedByMe) return;
    this.noticeService.acknowledgeNotice(notice.id).subscribe({
      next: () => { notice.isAcknowledgedByMe = true; notice.acknowledgementCount++; }
    });
  }

  deleteNotice(notice: NoticeResponse): void {
    if (!confirm(`Delete notice "${notice.title}"?`)) return;
    this.noticeService.deleteNotice(notice.id).subscribe({
      next: () => {
        this.notices = this.notices.filter(n => n.id !== notice.id);
        this.snackBar.open('Notice deleted.', 'Close', { duration: 3000 });
      }
    });
  }

  getTimeAgo(date: string): string {
    const diff = Date.now() - new Date(date).getTime();
    const hours = Math.floor(diff / 3600000);
    if (hours < 1) return 'Just now';
    if (hours < 24) return `${hours}h ago`;
    return `${Math.floor(hours/24)}d ago`;
  }
}