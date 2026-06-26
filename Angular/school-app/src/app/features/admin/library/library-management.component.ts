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

import { LibraryService } from '../../../core/services/library.service';
import { AdminService } from '../../../core/services/admin.service';
import {
  LibraryBookResponse, BookIssueResponse
} from '../../../core/models/library.models';
import { StudentResponse } from '../../../core/models/admin.models';

@Component({
  selector: 'app-library-management',
  standalone: true,
  imports: [
    CommonModule, ReactiveFormsModule, FormsModule,
    MatTabsModule, MatFormFieldModule, MatInputModule,
    MatSelectModule, MatButtonModule, MatIconModule,
    MatSnackBarModule, MatProgressSpinnerModule
  ],
  templateUrl: './library-management.component.html',
  styleUrls: ['./library-management.component.scss']
})
export class LibraryManagementComponent implements OnInit {
  private libraryService = inject(LibraryService);
  private adminService   = inject(AdminService);
  private snackBar       = inject(MatSnackBar);
  private fb             = inject(FormBuilder);

  addBookForm!: FormGroup;
  issueForm!:   FormGroup;

  books:        LibraryBookResponse[] = [];
  filteredBooks: LibraryBookResponse[] = [];
  activeIssues: BookIssueResponse[]   = [];
  overdueIssues: BookIssueResponse[]  = [];
  students:     StudentResponse[]     = [];

  searchTerm  = '';
  isLoading   = signal(false);
  isAdding    = signal(false);
  isIssuing   = signal(false);
  showAddForm = signal(false);

  classes: any[] = [];

  ngOnInit(): void {
    this.addBookForm = this.fb.group({
      title:         ['', Validators.required],
      author:        ['', Validators.required],
      isbn:          ['', Validators.required],
      category:      ['', Validators.required],
      publisher:     [''],
      publishedYear: [''],
      totalCopies:   [1, [Validators.required, Validators.min(1)]],
      description:   [''],
      shelfLocation: ['']
    });

    this.issueForm = this.fb.group({
      bookId:    ['', Validators.required],
      studentId: ['', Validators.required],
      dueDate:   ['', Validators.required],
      remarks:   ['']
    });

    this.loadData();
    this.adminService.getAllClasses().subscribe({ next: d => this.classes = d });
  }

  private loadData(): void {
    this.isLoading.set(true);
    this.libraryService.getAllBooks().subscribe({
      next: d => {
        this.books = d;
        this.filteredBooks = d;
        this.isLoading.set(false);
      },
      error: () => this.isLoading.set(false)
    });

    this.libraryService.getActiveIssues().subscribe({
      next: d => this.activeIssues = d
    });

    this.libraryService.getOverdueIssues().subscribe({
      next: d => this.overdueIssues = d
    });
  }

  filterBooks(): void {
    const t = this.searchTerm.toLowerCase();
    this.filteredBooks = this.books.filter(b =>
      b.title.toLowerCase().includes(t)  ||
      b.author.toLowerCase().includes(t) ||
      b.isbn.includes(t)                 ||
      b.category.toLowerCase().includes(t)
    );
  }

  loadStudentsForClass(classId: string): void {
    this.adminService.getStudentsByClass(classId).subscribe({
      next: d => this.students = d
    });
  }

  addBook(): void {
    if (this.addBookForm.invalid) {
      this.addBookForm.markAllAsTouched(); return;
    }
    this.isAdding.set(true);

    this.libraryService.addBook(this.addBookForm.value).subscribe({
      next: (res) => {
        this.books.unshift(res);
        this.filteredBooks = this.books;
        this.addBookForm.reset({ totalCopies: 1 });
        this.showAddForm.set(false);
        this.snackBar.open('Book added!', 'Close', {
          duration: 3000, panelClass: ['success-snackbar']
        });
        this.isAdding.set(false);
      },
      error: (err) => {
        this.snackBar.open(err.error?.message ?? 'Failed.', 'Close', {
          duration: 4000, panelClass: ['error-snackbar']
        });
        this.isAdding.set(false);
      }
    });
  }

  issueBook(): void {
    if (this.issueForm.invalid) {
      this.issueForm.markAllAsTouched(); return;
    }
    this.isIssuing.set(true);
    const val = this.issueForm.value;
    const dto = {
      ...val,
      dueDate: val.dueDate instanceof Date
        ? val.dueDate.toISOString() : val.dueDate
    };

    this.libraryService.issueBook(dto).subscribe({
      next: (res) => {
        this.activeIssues.unshift(res);
        this.issueForm.reset();
        // Reduce available count
        const book = this.books.find(b => b.id === val.bookId);
        if (book) book.availableCopies--;
        this.snackBar.open('Book issued!', 'Close', {
          duration: 3000, panelClass: ['success-snackbar']
        });
        this.isIssuing.set(false);
      },
      error: (err) => {
        this.snackBar.open(err.error?.message ?? 'Failed.', 'Close', {
          duration: 4000, panelClass: ['error-snackbar']
        });
        this.isIssuing.set(false);
      }
    });
  }

  returnBook(issue: BookIssueResponse): void {
    if (!confirm(`Return "${issue.bookTitle}" from ${issue.studentName}?`)) return;

    this.libraryService.returnBook(issue.id).subscribe({
      next: (res) => {
        this.activeIssues = this.activeIssues.filter(i => i.id !== issue.id);
        const book = this.books.find(b => b.title === issue.bookTitle);
        if (book) book.availableCopies++;

        let msg = 'Book returned.';
        if (res.fineAmount > 0) msg += ` Fine: ₹${res.fineAmount}`;
        this.snackBar.open(msg, 'Close', { duration: 4000 });
      }
    });
  }

  deleteBook(book: LibraryBookResponse): void {
    if (!confirm(`Delete "${book.title}"?`)) return;
    this.libraryService.deleteBook(book.id).subscribe({
      next: () => {
        this.books = this.books.filter(b => b.id !== book.id);
        this.filteredBooks = this.filteredBooks.filter(b => b.id !== book.id);
        this.snackBar.open('Book deleted.', 'Close', { duration: 3000 });
      },
      error: (err) => {
        this.snackBar.open(err.error?.message ?? 'Cannot delete.', 'Close', { duration: 4000 });
      }
    });
  }

  getIssueDays(dueDate: string): number {
    const now  = new Date().getTime();
    const due  = new Date(dueDate).getTime();
    return Math.ceil((due - now) / 86400000);
  }
}