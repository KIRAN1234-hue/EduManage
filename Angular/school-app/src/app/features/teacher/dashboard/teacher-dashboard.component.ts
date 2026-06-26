import { Component, OnInit, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { MatIconModule } from '@angular/material/icon';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { Router } from '@angular/router';
import { DashboardService } from '../../../core/services/dashboard.service';
import { DashboardStats } from '../../../core/models/dashboard.models';
import { Store } from '@ngrx/store';
import { selectCurrentUser } from '../../../core/store/auth/auth.selectors';

@Component({
  selector: 'app-teacher-dashboard',
  standalone: true,
  imports: [CommonModule, MatIconModule, MatProgressSpinnerModule],
  templateUrl: './teacher-dashboard.component.html',
  styleUrls: ['./teacher-dashboard.component.scss']
})
export class TeacherDashboardComponent implements OnInit {
  private dashboardService = inject(DashboardService);
  private router           = inject(Router);
  private store            = inject(Store);

  today = new Date();
  user$     = this.store.select(selectCurrentUser);
  stats     = signal<DashboardStats | null>(null);
  isLoading = signal(true);

  ngOnInit(): void {
    this.dashboardService.getTeacherStats().subscribe({
      next: (data) => { this.stats.set(data); this.isLoading.set(false); },
      error: ()     => this.isLoading.set(false)
    });
  }

  navigateTo(route: string): void { this.router.navigate([route]); }
}