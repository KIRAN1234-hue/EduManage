import { HttpInterceptorFn } from '@angular/common/http';
import { inject } from '@angular/core';
import { finalize } from 'rxjs/operators';
import { LoadingService } from '../services/loading.service';

// URLs that should NOT show global spinner (polling/silent calls)
const SKIP_URLS = [
  'unread-count',
  'notifications',
  'analytics',
  'dashboard'
];

export const loadingInterceptor: HttpInterceptorFn = (req, next) => {
  const loadingService = inject(LoadingService);

  const shouldSkip = SKIP_URLS.some(url => req.url.includes(url));
  if (shouldSkip) return next(req);

  loadingService.show();

  return next(req).pipe(
    finalize(() => loadingService.hide())
  );
};