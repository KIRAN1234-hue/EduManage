import { Component } from '@angular/core';
import { Router } from '@angular/router';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';

@Component({
  selector: 'app-unauthorized',
  standalone: true,
  imports: [MatButtonModule, MatIconModule],
  template: `
    <div class="unauth-page">
      <div class="unauth-card">
        <div class="error-code">403</div>
        <mat-icon class="lock-icon">lock</mat-icon>
        <h2>Access Denied</h2>
        <p>You do not have permission to access this page.</p>
        <button mat-raised-button color="primary" (click)="goBack()">
          <mat-icon>arrow_back</mat-icon>
          Go Back
        </button>
      </div>
    </div>
  `,
  styles: [`
    .unauth-page {
      min-height: 100vh;
      display: flex; align-items: center; justify-content: center;
      background: #FFF1F2;
    }
    .unauth-card {
      text-align: center; padding: 3rem;
      background: white; border-radius: 20px;
      box-shadow: 0 10px 40px rgba(0,0,0,0.08);
    }
    .error-code {
      font-size: 5rem; font-weight: 900;
      color: #FCA5A5; line-height: 1;
    }
    .lock-icon {
      font-size: 48px; width: 48px; height: 48px;
      color: #DC2626; margin: 1rem 0;
    }
    h2 { color: #0F172A; margin: 0 0 0.5rem; }
    p { color: #64748B; margin-bottom: 1.5rem; }
  `]
})
export class UnauthorizedComponent {
  constructor(private router: Router) {}
  goBack() { this.router.navigate(['/login']); }
}