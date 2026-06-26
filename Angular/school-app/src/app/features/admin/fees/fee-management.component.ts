  import {
    Component, OnInit, inject, signal
  } from '@angular/core';
  import { CommonModule } from '@angular/common';
  import {
    ReactiveFormsModule, FormBuilder,
    FormGroup, Validators, FormsModule
  } from '@angular/forms';
  import { MatTabsModule } from '@angular/material/tabs';
  import { MatFormFieldModule } from '@angular/material/form-field';
  import { MatInputModule } from '@angular/material/input';
  import { MatSelectModule } from '@angular/material/select';
  import { MatButtonModule } from '@angular/material/button';
  import { MatIconModule } from '@angular/material/icon';
  import { MatSnackBar, MatSnackBarModule } from '@angular/material/snack-bar';
  import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
  import { MatDatepickerModule } from '@angular/material/datepicker';
  import { MatNativeDateModule } from '@angular/material/core';
  import { MatCheckboxModule } from '@angular/material/checkbox';

  import { FeeService } from '../../../core/services/fee.service';
  import { AdminService } from '../../../core/services/admin.service';
  import {
    FeeStructureResponse, FeePaymentResponse, StudentFeeStatus,FeeTermStatus
  } from '../../../core/models/fee.models';
  import { ClassResponse, StudentResponse } from '../../../core/models/admin.models';

  @Component({
    selector: 'app-fee-management',
    standalone: true,
    imports: [
      CommonModule, ReactiveFormsModule, FormsModule,
      MatTabsModule, MatFormFieldModule, MatInputModule,
      MatSelectModule, MatButtonModule, MatIconModule,
      MatSnackBarModule, MatProgressSpinnerModule,
      MatDatepickerModule, MatNativeDateModule, MatCheckboxModule
    ],
    templateUrl: './fee-management.component.html',
    styleUrls: ['./fee-management.component.scss']
  })
  export class FeeManagementComponent implements OnInit {
    private feeService   = inject(FeeService);
    private adminService = inject(AdminService);
    private snackBar     = inject(MatSnackBar);
    private fb           = inject(FormBuilder);

    structureForm!: FormGroup;
    paymentForm!:   FormGroup;

    classes:    ClassResponse[]      = [];
    students:   StudentResponse[]    = [];
    structures: FeeStructureResponse[] = [];
    payments:   FeePaymentResponse[] = [];
    studentFeeStatus: StudentFeeStatus | null = null;

isLoadingStructures = signal(false);
isCreatingStructure = signal(false);
isRecordingPayment = signal(false);
isLoadingPayments = signal(false);

// Existing payment flow (still used below)
studentBalance = signal<{
  totalAmount: number;
  paid: number;
  remaining: number;
  isFullyPaid: boolean;
} | null>(null);

isCheckingBalance = signal(false);

selectedStructureId = '';

// New properties from task
selectedClassForPayment = '';
selectedStudentForPayment = '';

paymentFeeStatus = signal<StudentFeeStatus | null>(null);
isLoadingPaymentStatus = signal(false);

expandedTermId = signal<string | null>(null);

termPaymentForms: Record<string, FormGroup> = {};

isSubmittingTerm = signal<string | null>(null);

studentsForPayment: StudentResponse[] = [];

paymentMethods = ['Cash', 'Online', 'Cheque', 'DD', 'Razorpay'];

academicYear = '2024-25';

    ngOnInit(): void {
      this.structureForm = this.fb.group({
        classId:         ['', Validators.required],
        academicYear:    [this.academicYear, Validators.required],
        termName:        ['', Validators.required],
        amount:          ['', [Validators.required, Validators.min(1)]],
        dueDate:         ['', Validators.required],
        discountAllowed: [false],
        description:     ['']
      });

      this.paymentForm = this.fb.group({
        feeStructureId: ['', Validators.required],
        studentId:      ['', Validators.required],
        amount:         ['', [Validators.required, Validators.min(1)]],
        discountAmount: [0],
        paymentMethod:  ['Cash', Validators.required],
        remarks:        [''],
        receiptUrl:     ['']
      });

      this.loadData();

// Watch BOTH feeStructureId and studentId changes
const loadBalance = () => {
  const structureId = this.paymentForm.get('feeStructureId')?.value;
  const studentId = this.paymentForm.get('studentId')?.value;

  if (!structureId || !studentId) {
    this.studentBalance.set(null);
    return;
  }

  this.isCheckingBalance.set(true);

  this.feeService.getStudentBalance(studentId, structureId).subscribe({
    next: (balance) => {
      this.studentBalance.set(balance);
      this.isCheckingBalance.set(false);

      const amountControl = this.paymentForm.get('amount');

      if (balance.isFullyPaid) {
        amountControl?.setValue(0);
        amountControl?.disable();
      } else {
        amountControl?.enable();
        amountControl?.setValidators([
          Validators.required,
          Validators.min(1),
          Validators.max(balance.remaining)
        ]);

        amountControl?.setValue(balance.remaining);
        amountControl?.updateValueAndValidity();
      }
    },
    error: () => this.isCheckingBalance.set(false)
  });
};

this.paymentForm.get('feeStructureId')?.valueChanges.subscribe(structureId => {
  const structure = this.structures.find(s => s.id === structureId);

  if (structure) {
    this.loadStudentsForStructure(structure.className);
  }

  loadBalance();
});

this.paymentForm.get('studentId')?.valueChanges.subscribe(() => {
  loadBalance();
});
    }

    private loadData(): void {
      this.adminService.getAllClasses().subscribe({ next: d => this.classes = d });
      this.loadStructures();
    }

    loadStructures(): void {
      this.isLoadingStructures.set(true);
      this.feeService.getFeeStructures(this.academicYear).subscribe({
        next: d => { this.structures = d; this.isLoadingStructures.set(false); },
        error: () => this.isLoadingStructures.set(false)
      });
    }

    private loadStudentsForStructure(className: string): void {
      const cls = this.classes.find(c =>
        `${c.name} ${c.section}`.trim() === className.trim()
      );
      if (cls) {
        this.adminService.getStudentsByClass(cls.id).subscribe({
          next: d => this.students = d
        });
      }
    }

    createStructure(): void {
      if (this.structureForm.invalid) {
        this.structureForm.markAllAsTouched(); return;
      }
      this.isCreatingStructure.set(true);
      const val = this.structureForm.value;
      const dto = {
        ...val,
        dueDate: val.dueDate instanceof Date
          ? this.formatDate(val.dueDate) : val.dueDate
      };

      this.feeService.createFeeStructure(dto).subscribe({
        next: (res) => {
          this.structures.unshift(res);
          this.structureForm.reset({
            academicYear: this.academicYear,
            discountAllowed: false
          });
          this.snackBar.open('Fee structure created!', 'Close', {
            duration: 3000, panelClass: ['success-snackbar']
          });
          this.isCreatingStructure.set(false);
        },
        error: (err) => {
          this.snackBar.open(err.error?.message ?? 'Failed to create.', 'Close', {
            duration: 4000, panelClass: ['error-snackbar']
          });
          this.isCreatingStructure.set(false);
        }
      });
    }

    recordPayment(): void {
      if (this.paymentForm.invalid) {
        this.paymentForm.markAllAsTouched(); return;
      }
      this.isRecordingPayment.set(true);

      this.feeService.recordPayment(this.paymentForm.value).subscribe({
        next: () => {
          this.paymentForm.patchValue({
            studentId: '', amount: '', discountAmount: 0, remarks: ''
          });
          this.snackBar.open('Payment recorded!', 'Close', {
            duration: 3000, panelClass: ['success-snackbar']
          });
          this.isRecordingPayment.set(false);
          this.loadStructures();
        },
        error: (err) => {
          this.snackBar.open(err.error?.message ?? 'Failed.', 'Close', {
            duration: 4000, panelClass: ['error-snackbar']
          });
          this.isRecordingPayment.set(false);
        }
      });
    }

    viewPayments(structureId: string): void {
      this.selectedStructureId = structureId;
      this.isLoadingPayments.set(true);
      this.feeService.getPaymentsByStructure(structureId).subscribe({
        next: d => { this.payments = d; this.isLoadingPayments.set(false); },
        error: () => this.isLoadingPayments.set(false)
      });
    }

    viewStudentStatus(studentId: string): void {
      this.feeService.getStudentFeeStatus(studentId, this.academicYear).subscribe({
        next: d => this.studentFeeStatus = d
      });
    }

    onPaymentClassChange(classId: string): void {
  this.selectedStudentForPayment = '';
  this.paymentFeeStatus.set(null);
  this.expandedTermId.set(null);
  this.termPaymentForms = {};

  this.adminService.getStudentsByClass(classId).subscribe({
    next: d => this.studentsForPayment = d
  });
}

onPaymentStudentChange(studentId: string): void {
  if (!studentId) return;
  this.expandedTermId.set(null);
  this.termPaymentForms = {};
  this.isLoadingPaymentStatus.set(true);

  this.feeService.getStudentFeeStatus(studentId, this.academicYear).subscribe({
    next: (status) => {
      this.paymentFeeStatus.set(status);
      this.isLoadingPaymentStatus.set(false);

      // Pre-build payment forms for actionable terms only
      status.terms
        .filter(t => t.status !== 'Paid')
        .forEach(term => {
          const remaining = term.amount - term.paid;
          this.termPaymentForms[term.feeStructureId] = this.fb.group({
            amount:        [remaining, [Validators.required, Validators.min(1), Validators.max(remaining)]],
            discountAmount:[0],
            paymentMethod: ['Cash', Validators.required],
            remarks:       ['']
          });
        });
    },
    error: () => this.isLoadingPaymentStatus.set(false)
  });
}

toggleTermPayment(feeStructureId: string): void {
  this.expandedTermId.set(
    this.expandedTermId() === feeStructureId ? null : feeStructureId
  );
}

recordTermPayment(term: FeeTermStatus): void {
  const form = this.termPaymentForms[term.feeStructureId];
  if (!form || form.invalid) { form?.markAllAsTouched(); return; }

  this.isSubmittingTerm.set(term.feeStructureId);

  const dto = {
    studentId:      this.selectedStudentForPayment,
    feeStructureId: term.feeStructureId,
    amount:         form.value.amount,
    discountAmount: form.value.discountAmount || 0,
    paymentMethod:  form.value.paymentMethod,
    remarks:        form.value.remarks
  };

  this.feeService.recordPayment(dto).subscribe({
    next: () => {
      this.expandedTermId.set(null);
      this.snackBar.open('Payment recorded!', 'Close', {
        duration: 3000, panelClass: ['success-snackbar']
      });
      this.isSubmittingTerm.set(null);
      // Refresh status
      this.onPaymentStudentChange(this.selectedStudentForPayment);
      this.loadStructures();
    },
    error: (err) => {
      this.snackBar.open(err.error?.message ?? 'Failed.', 'Close', {
        duration: 4000, panelClass: ['error-snackbar']
      });
      this.isSubmittingTerm.set(null);
    }
  });
}

getTermPaidPercent(term: FeeTermStatus): number {
  return term.amount > 0 ? Math.min(100, Math.round(term.paid / term.amount * 100)) : 0;
}

    getStatusColor(status: string): string {
      const m: Record<string,string> = {
        Paid: '#16A34A', Pending: '#D97706', Overdue: '#DC2626', Partial: '#2563EB'
      };
      return m[status] ?? '#64748B';
    }

    getStatusBg(status: string): string {
      const m: Record<string,string> = {
        Paid: '#DCFCE7', Pending: '#FEF3C7', Overdue: '#FEE2E2', Partial: '#DBEAFE'
      };
      return m[status] ?? '#F1F5F9';
    }

    private formatDate(d: Date): string {
      const y = d.getFullYear();
      const m = String(d.getMonth()+1).padStart(2,'0');
      const day = String(d.getDate()).padStart(2,'0');
      return `${y}-${m}-${day}`;
    }
  }