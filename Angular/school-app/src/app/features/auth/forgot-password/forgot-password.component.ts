import { Component, OnInit, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ReactiveFormsModule, FormBuilder, FormGroup, Validators } from '@angular/forms';
import { Router } from '@angular/router';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { HttpClient } from '@angular/common/http';
import { environment } from '../../../../environments/environment';

@Component({
  selector: 'app-forgot-password',
  standalone: true,
  imports: [
    CommonModule, ReactiveFormsModule,
    MatFormFieldModule, MatInputModule,
    MatButtonModule, MatIconModule,
    MatProgressSpinnerModule
  ],
  template: `
    <div class="fp-page">
      <div class="fp-card">
        <div class="fp-logo">
          <div class="logo-circle"><mat-icon>lock_reset</mat-icon></div>
          <h1>EduManage</h1>
        </div>

        <div class="success-state" *ngIf="success()">
          <mat-icon class="success-icon">check_circle</mat-icon>
          <h2>Token Generated</h2>
          <p>Share the reset token with the user to complete password reset.</p>
          <div class="token-box" *ngIf="resetToken()">
            <p class="token-label">Reset Token:</p>
            <code class="token-val">{{ resetToken() }}</code>
            <button mat-stroked-button (click)="copyToken()">
              <mat-icon>content_copy</mat-icon> Copy
            </button>
          </div>
          <button mat-stroked-button (click)="router.navigate(['/login'])">
            Back to Login
          </button>
        </div>

        <div *ngIf="!success()">
          <div class="fp-header">
            <h2>Forgot Password</h2>
            <p>Enter email to generate a reset token.</p>
          </div>

          <form [formGroup]="form" (ngSubmit)="onSubmit()">
            <mat-form-field appearance="outline" class="full-w">
              <mat-label>Email Address</mat-label>
              <mat-icon matPrefix>email</mat-icon>
              <input matInput type="email" formControlName="email"/>
            </mat-form-field>

            <button mat-raised-button type="submit"
              class="submit-btn" [disabled]="isLoading()">
              <mat-spinner *ngIf="isLoading()" diameter="20"></mat-spinner>
              <mat-icon *ngIf="!isLoading()">send</mat-icon>
              {{ isLoading() ? 'Processing...' : 'Get Reset Token' }}
            </button>

            <p class="error-msg" *ngIf="errorMsg()">{{ errorMsg() }}</p>
          </form>

          <p class="back-link">
            <a (click)="router.navigate(['/login'])">← Back to Login</a>
          </p>
        </div>
      </div>
    </div>
  `,
  styles: [`
    .fp-page {
      min-height: 100vh;
      display: flex;
      align-items: center;
      justify-content: center;
      background: linear-gradient(145deg, #1E3A8A, #2563EB 60%, #7C3AED);
      padding: 24px;
    }
    .fp-card {
      width: 100%;
      max-width: 420px;
      background: white;
      border-radius: 24px;
      padding: 2.5rem;
      box-shadow: 0 25px 60px rgba(0,0,0,0.25);
    }
    .fp-logo { display:flex; flex-direction:column; align-items:center; gap:8px; margin-bottom:28px; h1 { font-size:1.3rem; font-weight:800; color:#0F172A; margin:0; } }
    .logo-circle { width:52px; height:52px; background:linear-gradient(135deg,#2563EB,#7C3AED); border-radius:14px; display:flex; align-items:center; justify-content:center; mat-icon { color:white; font-size:28px; width:28px; height:28px; } }
    .fp-header { margin-bottom:24px; text-align:center; h2 { font-size:1.3rem; font-weight:800; color:#0F172A; margin:0 0 6px; } p { font-size:0.85rem; color:#64748B; margin:0; } }
    .full-w { width:100%; margin-bottom:8px; }
    .submit-btn { width:100%; height:50px; margin-top:8px; font-size:0.95rem; font-weight:600; background:linear-gradient(135deg,#2563EB,#7C3AED) !important; color:white !important; border-radius:12px !important; display:flex; align-items:center; justify-content:center; gap:8px; }
    .error-msg { color:#DC2626; font-size:0.82rem; text-align:center; margin-top:8px; }
    .back-link { text-align:center; margin-top:16px; font-size:0.82rem; a { color:#2563EB; cursor:pointer; } }
    .success-state { display:flex; flex-direction:column; align-items:center; gap:12px; text-align:center; h2 { font-size:1.2rem; font-weight:700; color:#0F172A; margin:0; } p { color:#64748B; font-size:0.875rem; margin:0; } }
    .success-icon { font-size:56px; width:56px; height:56px; color:#16A34A; }
    .token-box { width:100%; background:#F8FAFC; border:1px solid #E2E8F0; border-radius:10px; padding:14px; display:flex; flex-direction:column; gap:8px; }
    .token-label { font-size:0.72rem; font-weight:700; color:#94A3B8; text-transform:uppercase; margin:0; }
    .token-val { font-family:monospace; font-size:0.72rem; color:#2563EB; word-break:break-all; background:#EFF6FF; padding:8px; border-radius:6px; }
  `]
})
export class ForgotPasswordComponent implements OnInit {
  router     = inject(Router);
  private http = inject(HttpClient);
  private fb   = inject(FormBuilder);

  form!: FormGroup;
  isLoading = signal(false);
  success   = signal(false);
  errorMsg  = signal('');
  resetToken = signal('');

  ngOnInit(): void {
    this.form = this.fb.group({
      email: ['', [Validators.required, Validators.email]]
    });
  }

  onSubmit(): void {
    if (this.form.invalid) return;
    this.isLoading.set(true);
    this.errorMsg.set('');

    this.http.post<any>(
      `${environment.apiUrl}/auth/forgot-password`,
      this.form.value
    ).subscribe({
      next: (res) => {
        this.resetToken.set(res.resetToken ?? '');
        this.success.set(true);
        this.isLoading.set(false);
      },
      error: (err) => {
        this.errorMsg.set(err.error?.message ?? 'Failed.');
        this.isLoading.set(false);
      }
    });
  }

  copyToken(): void {
    navigator.clipboard.writeText(this.resetToken());
  }
}