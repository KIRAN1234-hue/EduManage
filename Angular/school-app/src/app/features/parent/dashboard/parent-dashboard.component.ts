import { Component, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { MatIconModule } from '@angular/material/icon';
import { Store } from '@ngrx/store';
import { selectCurrentUser } from '../../../core/store/auth/auth.selectors';

@Component({
  selector: 'app-parent-dashboard',
  standalone: true,
  imports: [CommonModule, MatIconModule],
  template: `
    <div class="coming-soon">
      <div class="cs-card">
        <div class="cs-icon">
          <mat-icon>family_restroom</mat-icon>
        </div>

        <h2>Parent Dashboard</h2>

        <p>
          Welcome,
          <strong>{{ (user$ | async)?.fullName }}</strong>
        </p>

        <p class="cs-note">
          Full parent dashboard coming in Week 3 Angular.
        </p>

        <div class="cs-modules">
          <span>Child Attendance</span>
          <span>Fee Payments</span>
          <span>Teacher Meetings</span>
          <span>Progress Reports</span>
        </div>
      </div>
    </div>
  `,
  styles: [`
    .coming-soon {
      display: flex;
      align-items: center;
      justify-content: center;
      min-height: 60vh;
    }

    .cs-card {
      text-align: center;
      padding: 3rem;
      background: white;
      border-radius: 20px;
      box-shadow: 0 4px 20px rgba(0,0,0,0.06);
      max-width: 400px;
      width: 100%;
    }

    .cs-icon {
      width: 72px;
      height: 72px;
      border-radius: 50%;
      background: #FEF3C7;
      display: flex;
      align-items: center;
      justify-content: center;
      margin: 0 auto 1.5rem;

      mat-icon {
        font-size: 36px;
        width: 36px;
        height: 36px;
        color: #D97706;
      }
    }

    h2 {
      color: #0F172A;
      margin: 0 0 0.5rem;
    }

    p {
      color: #64748B;
      margin: 0.25rem 0;
    }

    .cs-note {
      font-size: 0.85rem;
      margin-bottom: 1.5rem;
    }

    .cs-modules {
      display: flex;
      flex-wrap: wrap;
      gap: 8px;
      justify-content: center;

      span {
        padding: 4px 12px;
        background: #FFFBEB;
        border: 1px solid #FCD34D;
        border-radius: 20px;
        font-size: 0.78rem;
        color: #D97706;
      }
    }
  `]
})
export class ParentDashboardComponent {
  private store = inject(Store);
  user$ = this.store.select(selectCurrentUser);
}