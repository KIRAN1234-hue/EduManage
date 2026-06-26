import { inject } from '@angular/core';
import { CanActivateFn, Router, ActivatedRouteSnapshot } from '@angular/router';
import { Store } from '@ngrx/store';
import { map, take } from 'rxjs/operators';
import { selectUserRole } from '../store/auth/auth.selectors';

export const roleGuard = (allowedRole: string): CanActivateFn => {
  return (route: ActivatedRouteSnapshot) => {
    const store = inject(Store);
    const router = inject(Router);

    return store.select(selectUserRole).pipe(
      take(1),
      map(role => {
        if (role?.toLowerCase() === allowedRole.toLowerCase()) return true;
        router.navigate(['/unauthorized']);
        return false;
      })
    );
  };
};