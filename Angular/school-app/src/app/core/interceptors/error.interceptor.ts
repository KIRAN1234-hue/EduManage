import { HttpInterceptorFn, HttpErrorResponse } from '@angular/common/http';
import { inject } from '@angular/core';
import { Router } from '@angular/router';
import { Store } from '@ngrx/store';
import { catchError, throwError } from 'rxjs';
import { logout } from '../store/auth/auth.actions';

export const errorInterceptor: HttpInterceptorFn = (req, next) => {
  const router = inject(Router);
  const store = inject(Store);

  return next(req).pipe(
    catchError((error: HttpErrorResponse) => {
      switch (error.status) {
        case 401:
          // Token expired or invalid — logout and redirect
          store.dispatch(logout());
          break;
        case 403:
          // Authenticated but not authorized
          router.navigate(['/unauthorized']);
          break;
        case 0:
          console.error('Network error — API unreachable');
          break;
      }
      return throwError(() => error);
    })
  );
};