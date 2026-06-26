export interface LibraryBookResponse {
  id: string;
  title: string;
  author: string;
  isbn: string;
  category: string;
  publisher: string;
  publishedYear: number;
  totalCopies: number;
  availableCopies: number;
  description?: string;
  shelfLocation?: string;
  isAvailable: boolean;
}

export interface CreateBookRequest {
  title: string;
  author: string;
  isbn: string;
  category: string;
  publisher: string;
  publishedYear: number;
  totalCopies: number;
  description?: string;
  shelfLocation?: string;
}

export interface BookIssueResponse {
  id: string;
  bookTitle: string;
  bookAuthor: string;
  isbn: string;
  studentName: string;
  rollNumber: string;
  issuedAt: string;
  dueDate: string;
  returnedAt?: string;
  fineAmount: number;
  isReturned: boolean;
  status: string;
  remarks?: string;
  isOverdue: boolean;
}

export interface IssueBookRequest {
  bookId: string;
  studentId: string;
  dueDate: string;
  remarks?: string;
}