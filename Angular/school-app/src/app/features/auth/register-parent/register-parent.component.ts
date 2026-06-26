import { Component, OnInit, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import {
  ReactiveFormsModule, FormBuilder,
  FormGroup, Validators
} from '@angular/forms';
import { Router } from '@angular/router';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatSnackBar, MatSnackBarModule } from '@angular/material/snack-bar';
import { AdminService } from '../../../core/services/admin.service';

@Component({
  selector: 'app-register-parent',
  standalone: true,
  imports: [
    CommonModule, ReactiveFormsModule,
    MatFormFieldModule, MatInputModule,
    MatButtonModule, MatIconModule,
    MatProgressSpinnerModule, MatSnackBarModule
  ],
  templateUrl: './register-parent.component.html',
  styleUrls: ['./register-parent.component.scss']
})
export class RegisterParentComponent implements OnInit {
  private fb           = inject(FormBuilder);
  private adminService = inject(AdminService);
  private snackBar     = inject(MatSnackBar);
  router               = inject(Router);

  form!: FormGroup;
  isLoading    = signal(false);
  hidePassword = true;
  isSuccess    = signal(false);

  ngOnInit(): void {
    this.form = this.fb.group({
      fullName:          ['', Validators.required],
      email:             ['', [Validators.required, Validators.email]],
      password:          ['', [Validators.required, Validators.minLength(8)]],
      studentRollNumber: ['', Validators.required],
      studentEmail:      ['', [Validators.required, Validators.email]],
      phoneNumber:       ['']
    });
  }

  onSubmit(): void {
    if (this.form.invalid) { this.form.markAllAsTouched(); return; }
    this.isLoading.set(true);

    const val = this.form.value;
    const dto = { ...val, phoneNumber: val.phoneNumber || undefined };

    this.adminService.registerParent(dto).subscribe({
      next: () => {
        this.isSuccess.set(true);
        this.isLoading.set(false);
        setTimeout(() => this.router.navigate(['/login']), 3000);
      },
      error: (err) => {
        this.snackBar.open(
          err.error?.message ?? 'Registration failed. Check your details.',
          'Close',
          { duration: 5000, panelClass: ['error-snackbar'] }
        );
        this.isLoading.set(false);
      }
    });
  }
}