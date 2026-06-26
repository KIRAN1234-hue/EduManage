import { Component, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { LoadingService } from '../../../core/services/loading.service';

@Component({
  selector: 'app-loading-spinner',
  standalone: true,
  imports: [CommonModule],
  template: `
    <div class="global-loader" *ngIf="loadingService.isLoading()">
      <div class="loader-bar"></div>
    </div>
  `,
  styles: [`
    .global-loader {
      position: fixed;
      top: 0;
      left: 0;
      width: 100%;
      z-index: 9999;
      pointer-events: none;
    }

    .loader-bar {
      height: 3px;
      background: linear-gradient(90deg, #2563EB, #7C3AED, #2563EB);
      background-size: 200% 100%;
      animation: loading 1.5s linear infinite;
      border-radius: 0 2px 2px 0;
    }

    @keyframes loading {
      0%   { background-position: 200% 0; width: 30%; }
      50%  { width: 70%; }
      100% { background-position: -200% 0; width: 100%; }
    }
  `]
})
export class LoadingSpinnerComponent {
  loadingService = inject(LoadingService);
}