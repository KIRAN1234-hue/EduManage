import {
  Component, OnInit, inject
} from '@angular/core';
import { CommonModule } from '@angular/common';
import { ReactiveFormsModule, FormBuilder, FormGroup, Validators } from '@angular/forms';
import { Router } from '@angular/router';
import { Store } from '@ngrx/store';
import { MatIconModule } from '@angular/material/icon';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { login } from '../../../core/store/auth/auth.actions';
import { selectIsLoading, selectAuthError } from '../../../core/store/auth/auth.selectors';
import { signal } from '@angular/core';

@Component({
  selector: 'app-login',
  standalone: true,
  imports: [
    CommonModule, ReactiveFormsModule,
    MatIconModule, MatProgressSpinnerModule
  ],
  templateUrl: './login.component.html',
  styleUrls: ['./login.component.scss']
})
export class LoginComponent implements OnInit {
  private store = inject(Store);
  router        = inject(Router);
  private fb    = inject(FormBuilder);

  loginForm!: FormGroup;

  loading$ = this.store.select(selectIsLoading);
  error$   = this.store.select(selectAuthError);

  loginError = signal('');
isSubmitting = signal(false);


  showPassword  = false;
  selectedRole  = 'principal';

  roles = [
    { value: 'principal', label: 'Principal', icon: 'admin_panel_settings' },
    { value: 'teacher',   label: 'Teacher',   icon: 'person'               },
    { value: 'student',   label: 'Student',   icon: 'school'               },
    { value: 'parent',    label: 'Parent',    icon: 'family_restroom'       },
  ];

  ngOnInit(): void {
  this.loginForm = this.fb.group({
    email: ['', [Validators.required, Validators.email]],
    password: ['', Validators.required]
  });

  this.error$.subscribe(error => {
    console.log('STORE ERROR =', error);

    if (error) {
      this.loginError.set(error);
      this.isSubmitting.set(false);
    }
  });

  this.loading$.subscribe(loading => {
    if (!loading && !this.loginError()) {
      this.isSubmitting.set(false);
    }
  });
}

   

  onSubmit(): void {
    if (this.loginForm.invalid) {
      this.loginForm.markAllAsTouched();
      return;
    }
    const { email, password } = this.loginForm.value;
    this.store.dispatch(
  login({
    request: {
      email,
      password
    }
  })
);
  }
}