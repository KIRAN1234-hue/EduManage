import {
  Component, OnInit, inject, signal
} from '@angular/core';
import { CommonModule } from '@angular/common';
import {
  ReactiveFormsModule, FormBuilder,
  FormGroup, Validators
} from '@angular/forms';
import { MatIconModule } from '@angular/material/icon';
import { MatButtonModule } from '@angular/material/button';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatSelectModule } from '@angular/material/select';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatSnackBar, MatSnackBarModule } from '@angular/material/snack-bar';

import { FeeService } from '../../../core/services/fee.service';
import {
  StudentFeeStatus, FeeTermStatus,
  ParentPaymentRequest
} from '../../../core/models/fee.models';

@Component({
  selector: 'app-parent-fees',
  standalone: true,
  imports: [
    CommonModule, ReactiveFormsModule,
    MatIconModule, MatButtonModule,
    MatFormFieldModule, MatInputModule,
    MatSelectModule, MatProgressSpinnerModule,
    MatSnackBarModule
  ],
  templateUrl: './parent-fees.component.html',
  styleUrls: ['./parent-fees.component.scss']
})
export class ParentFeesComponent implements OnInit {
  private feeService = inject(FeeService);
  private snackBar   = inject(MatSnackBar);
  private fb         = inject(FormBuilder);

  feeStatus   = signal<StudentFeeStatus | null>(null);
  isLoading   = signal(true);
  isPaying    = signal(false);

  paymentForm!: FormGroup;

  selectedTerm   = signal<FeeTermStatus | null>(null);
  showPayDialog  = signal(false);

  paymentMethods = ['Online', 'UPI', 'Net Banking', 'Debit Card', 'Credit Card'];

  // We need feeStructureId — we'll get from the structures list
  feeStructures: any[] = [];

  ngOnInit(): void {
    this.paymentForm = this.fb.group({
      feeStructureId: ['', Validators.required],
      amount:         ['', [Validators.required, Validators.min(1)]],
      paymentMethod:  ['Online', Validators.required],
      remarks:        ['']
    });

    this.loadFeeStatus();
  }

  private loadFeeStatus(): void {
    this.isLoading.set(true);
    this.feeService.getMyChildFeeStatus().subscribe({
      next: d => {
        this.feeStatus.set(d);
        this.isLoading.set(false);
        // Load fee structures for the student's class to get IDs
        this.loadFeeStructures();
      },
      error: () => this.isLoading.set(false)
    });
  }

  private loadFeeStructures(): void {
    // Get structures to map term names to structure IDs
    this.feeService.getFeeStructures().subscribe({
      next: d => this.feeStructures = d
    });
  }

  openPayDialog(term: FeeTermStatus): void {
    this.selectedTerm.set(term);
    const structure = this.feeStructures.find(
      s => s.termName === term.termName
    );
    const balance = term.amount - term.paid;
    this.paymentForm.patchValue({
      feeStructureId: structure?.id ?? '',
      amount: balance,
      paymentMethod: 'Online',
      remarks: ''
    });
    this.showPayDialog.set(true);
  }

  submitPayment(): void {
    if (this.paymentForm.invalid) return;
    this.isPaying.set(true);

    const dto: ParentPaymentRequest = this.paymentForm.value;

    this.feeService.parentPayment(dto).subscribe({
      next: (res) => {
        this.showPayDialog.set(false);
        this.snackBar.open(
          `Payment of ₹${res.netAmount} recorded! Receipt: ${res.receiptNumber}`,
          'Close',
          { duration: 5000, panelClass: ['success-snackbar'] }
        );
        this.isPaying.set(false);
        this.loadFeeStatus();  // Refresh to show updated status
      },
      error: (err) => {
        this.snackBar.open(
          err.error?.message ?? 'Payment failed.',
          'Close',
          { duration: 4000, panelClass: ['error-snackbar'] }
        );
        this.isPaying.set(false);
      }
    });
  }

  getStatusColor(s: string): string {
    return { Paid:'#16A34A', Pending:'#D97706', Overdue:'#DC2626', Partial:'#2563EB' }[s] ?? '#64748B';
  }

  getStatusBg(s: string): string {
    return { Paid:'#DCFCE7', Pending:'#FEF3C7', Overdue:'#FEE2E2', Partial:'#DBEAFE' }[s] ?? '#F1F5F9';
  }

  getProgressWidth(paid: number, total: number): string {
    return total > 0 ? `${Math.min(100, Math.round(paid / total * 100))}%` : '0%';
  }

  canPay(term: FeeTermStatus): boolean {
    return term.status !== 'Paid' && term.amount - term.paid > 0;
  }
}