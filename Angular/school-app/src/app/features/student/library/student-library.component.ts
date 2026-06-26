import { Component, OnInit, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { MatIconModule } from '@angular/material/icon';
import { MatButtonModule } from '@angular/material/button';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatInputModule } from '@angular/material/input';
import { MatFormFieldModule } from '@angular/material/form-field';
import { LibraryService } from '../../../core/services/library.service';
import {
  LibraryBookResponse, BookIssueResponse
} from '../../../core/models/library.models';

@Component({
  selector: 'app-student-library',
  standalone: true,
  imports: [
    CommonModule, FormsModule,
    MatIconModule, MatButtonModule,
    MatProgressSpinnerModule, MatInputModule,
    MatFormFieldModule
  ],
  templateUrl: './student-library.component.html',
  styleUrls: ['./student-library.component.scss']
})
export class StudentLibraryComponent implements OnInit {
  private libraryService = inject(LibraryService);

  myIssues:      BookIssueResponse[]  = [];
  allBooks:      LibraryBookResponse[] = [];
  filteredBooks: LibraryBookResponse[] = [];
  searchTerm     = '';
  activeTab      = 'mybooks';
  isLoading      = signal(true);

  ngOnInit(): void {
    this.libraryService.getMyIssues().subscribe({
      next: d => {
        this.myIssues = d;
        this.isLoading.set(false);
      },
      error: () => this.isLoading.set(false)
    });

    this.libraryService.getAllBooks().subscribe({
      next: d => {
        this.allBooks = d;
        this.filteredBooks = d;
      }
    });
  }

  filterBooks(): void {
    const t = this.searchTerm.toLowerCase();
    this.filteredBooks = this.allBooks.filter(b =>
      b.title.toLowerCase().includes(t) ||
      b.author.toLowerCase().includes(t) ||
      b.category.toLowerCase().includes(t)
    );
  }

  get currentIssues(): BookIssueResponse[] {
    return this.myIssues.filter(i => !i.isReturned);
  }

  get returnedIssues(): BookIssueResponse[] {
    return this.myIssues.filter(i => i.isReturned);
  }

  getStatusColor(s: string): string {
    return { Issued:'#2563EB', Returned:'#16A34A', Overdue:'#DC2626' }[s] ?? '#64748B';
  }

  getStatusBg(s: string): string {
    return { Issued:'#DBEAFE', Returned:'#DCFCE7', Overdue:'#FEE2E2' }[s] ?? '#F1F5F9';
  }
}