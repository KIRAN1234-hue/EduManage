import { Injectable, inject } from '@angular/core';
import { Actions, createEffect, ofType } from '@ngrx/effects';
import { Router } from '@angular/router';
import { of } from 'rxjs';
import { catchError, map, switchMap, tap } from 'rxjs/operators';
import * as AuthActions from './auth.actions';
import { AuthService } from '../../services/auth.service';

@Injectable()
export class AuthEffects {
  private actions$ = inject(Actions);
  private authService = inject(AuthService);
  private router = inject(Router);

  // Login effect
  login$ = createEffect(() =>
    this.actions$.pipe(
      ofType(AuthActions.login),
      switchMap(({ request }) =>
        this.authService.login(request).pipe(
          map(response => AuthActions.loginSuccess({ response })),
          catchError(error =>
            of(AuthActions.loginFailure({
              error: error.error?.message ?? 'Login failed. Please try again.'
            }))
          )
        )
      )
    )
  );

  // Navigate after login success based on role
  loginSuccess$ = createEffect(() =>
    this.actions$.pipe(
      ofType(AuthActions.loginSuccess),
      tap(({ response }) => {
        // Save tokens to sessionStorage for page refresh persistence
        sessionStorage.setItem('accessToken', response.accessToken);
        sessionStorage.setItem('refreshToken', response.refreshToken);

        // Navigate based on role
        const role = response.role.toLowerCase();
        switch (role) {
          case 'principal': this.router.navigate(['/admin']); break;
          case 'teacher':   this.router.navigate(['/teacher']); break;
          case 'student':   this.router.navigate(['/student']); break;
          case 'parent':    this.router.navigate(['/parent']); break;
          default:          this.router.navigate(['/login']); break;
        }
      })
    ),
    { dispatch: false }
  );

  // Logout effect
  logout$ = createEffect(() =>
    this.actions$.pipe(
      ofType(AuthActions.logout),
      switchMap(() => {
        const refreshToken = sessionStorage.getItem('refreshToken') ?? '';
        return this.authService.logout(refreshToken).pipe(
          map(() => AuthActions.logoutSuccess()),
          catchError(() => of(AuthActions.logoutSuccess()))
        );
      })
    )
  );

  // Clear storage and redirect after logout
  logoutSuccess$ = createEffect(() =>
    this.actions$.pipe(
      ofType(AuthActions.logoutSuccess),
      tap(() => {
        sessionStorage.removeItem('accessToken');
        sessionStorage.removeItem('refreshToken');
        this.router.navigate(['/login']);
      })
    ),
    { dispatch: false }
  );
}