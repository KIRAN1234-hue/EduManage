import {
  Component, OnInit, inject, signal,
  ViewChild, ElementRef, AfterViewChecked
} from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule, ReactiveFormsModule, FormControl, Validators } from '@angular/forms';
import { MatIconModule } from '@angular/material/icon';
import { MatButtonModule } from '@angular/material/button';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatSelectModule } from '@angular/material/select';
import { MatSnackBar, MatSnackBarModule } from '@angular/material/snack-bar';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { Store } from '@ngrx/store';
import { filter, take } from 'rxjs/operators';

import { MessageService } from '../../../core/services/message.service';
import { AdminService } from '../../../core/services/admin.service';
import { MessageResponse, ConversationSummary } from '../../../core/models/message.models';
import { TeacherForDoubt } from '../../../core/models/admin.models';
import { selectCurrentUser,selectAccessToken } from '../../../core/store/auth/auth.selectors';

@Component({
  selector: 'app-student-doubts',
  standalone: true,
  imports: [
    CommonModule, FormsModule, ReactiveFormsModule,
    MatIconModule, MatButtonModule, MatFormFieldModule,
    MatInputModule, MatSelectModule, MatSnackBarModule,
    MatProgressSpinnerModule
  ],
  templateUrl: './student-doubts.component.html',
  styleUrls: ['./student-doubts.component.scss']
})
export class StudentDoubtsComponent implements OnInit, AfterViewChecked {
  @ViewChild('chatBody') chatBody!: ElementRef;

  private messageService = inject(MessageService);
  private adminService   = inject(AdminService);
  private snackBar       = inject(MatSnackBar);
  private store          = inject(Store);

  myUserId             = '';
  conversations:        ConversationSummary[] = [];
  activeMessages:       MessageResponse[]     = [];
  teachers:             TeacherForDoubt[]     = [];
  selectedConv          = signal<ConversationSummary | null>(null);
  isLoadingConvs        = signal(false);
  isLoadingMessages     = signal(false);
  isSending             = signal(false);
  showNewChat           = signal(false);
  searchTerm            = '';
  activeFilter          = 'all';

  replyControl    = new FormControl('', Validators.required);
  newMsgContent   = new FormControl('', Validators.required);
  newMsgTeacher   = new FormControl('', Validators.required);
  newMsgType      = new FormControl('Doubt');

  messageTypes = ['Doubt', 'General', 'Homework Help', 'Exam Query'];

  get filteredConversations(): ConversationSummary[] {
    let list = this.conversations;
    if (this.activeFilter === 'unread')
      list = list.filter(c => c.unreadCount > 0);
    if (this.searchTerm)
      list = list.filter(c =>
        c.otherUserName.toLowerCase().includes(this.searchTerm.toLowerCase()) ||
        c.lastMessage.toLowerCase().includes(this.searchTerm.toLowerCase())
      );
    return list;
  }

  ngOnInit(): void {

  this.store.select(selectAccessToken).pipe(
    filter(token => !!token),
    take(1)
  ).subscribe(token => {

    this.myUserId = this.parseJwtUserId(token!);

    console.log('JWT USER ID:', this.myUserId);

    this.loadConversations();
  });

  this.adminService.getTeachersForDoubts().subscribe({
    next: d => this.teachers = d
  });
}

  ngAfterViewChecked(): void {
    this.scrollToBottom();
  }

  loadConversations(): void {
    this.isLoadingConvs.set(true);
    this.messageService.getConversations().subscribe({
      next: d => { this.conversations = d; this.isLoadingConvs.set(false); },
      error: () => this.isLoadingConvs.set(false)
    });
  }

  openConversation(conv: ConversationSummary): void {
    this.selectedConv.set(conv);
    this.showNewChat.set(false);
    this.isLoadingMessages.set(true);

    this.messageService.getConversation(conv.otherUserId).subscribe({
      next: d => {
        this.activeMessages = d;
        conv.unreadCount = 0;
        this.isLoadingMessages.set(false);
        setTimeout(() => this.scrollToBottom(), 100);
      },
      error: () => this.isLoadingMessages.set(false)
    });
  }

  sendReply(): void {
    const content = this.replyControl.value?.trim();
    if (!content || !this.selectedConv()) return;
    this.isSending.set(true);

    this.messageService.sendMessage({
      receiverId:      this.selectedConv()!.otherUserId,
      subject: '',
      content:         content,
      messageType:     'Doubt',
      parentMessageId: undefined
    }).subscribe({
      next: (msg) => {
        this.activeMessages.push(msg);
        this.replyControl.reset();
        this.isSending.set(false);
        setTimeout(() => this.scrollToBottom(), 50);
      },
      error: () => this.isSending.set(false)
    });
  }

  sendNewMessage(): void {
    if (this.newMsgContent.invalid || this.newMsgTeacher.invalid) return;
    this.isSending.set(true);

    this.messageService.sendMessage({
      receiverId:      this.newMsgTeacher.value!,
      subject: '',
      content:         this.newMsgContent.value!,
      messageType:     this.newMsgType.value ?? 'Doubt',
      parentMessageId: undefined
    }).subscribe({
      next: () => {
        this.showNewChat.set(false);
        this.newMsgContent.reset();
        this.newMsgTeacher.reset();
        this.snackBar.open('Message sent!', 'Close', {
          duration: 3000, panelClass: ['success-snackbar']
        });
        this.isSending.set(false);
        this.loadConversations();
      },
      error: () => this.isSending.set(false)
    });
  }

  isMyMessage(msg: MessageResponse): boolean {

     console.log('----------------');
  console.log('MY USER ID:', this.myUserId);
  console.log('MESSAGE SENDER ID:', msg.senderId);
  console.log('MESSAGE RECEIVER ID:', msg.receiverId);
  console.log('MATCH:', msg.senderId === this.myUserId);

  
  if (!this.myUserId) return false;

  return msg.senderId === this.myUserId;
}

  getTotalUnread(): number {
  return this.conversations.reduce(
    (sum, c) => sum + c.unreadCount,
    0
  );
}

  getAvatarColor(name: string): string {
    const colors = [
      '#7C3AED','#2563EB','#16A34A',
      '#D97706','#DC2626','#0891B2','#DB2777'
    ];
    const idx = name.charCodeAt(0) % colors.length;
    return colors[idx];
  }

  getInitials(name: string): string {
    return name.split(' ').map(n => n[0]).join('').substring(0, 2).toUpperCase();
  }

  formatTime(dateStr: string): string {
    const d = new Date(dateStr);
    const now = new Date();
    const diffDays = Math.floor((now.getTime() - d.getTime()) / 86400000);
    if (diffDays === 0) return d.toLocaleTimeString([], { hour:'2-digit', minute:'2-digit' });
    if (diffDays === 1) return 'Yesterday';
    if (diffDays < 7) return d.toLocaleDateString([], { weekday: 'short' });
    return d.toLocaleDateString([], { month: 'short', day: 'numeric' });
  }

  formatMessageTime(dateStr: string): string {
    return new Date(dateStr).toLocaleTimeString([], {
      hour: '2-digit', minute: '2-digit'
    });
  }

  private parseJwtUserId(token: string): string {

  try {

    const base64 = token.split('.')[1];
    const payload = JSON.parse(atob(base64));

    console.log('JWT PAYLOAD', payload);

    return payload['http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier']
        || payload['nameid']
        || payload['sub']
        || '';

  } catch {

    return '';

  }
}


  private scrollToBottom(): void {
    try {
      if (this.chatBody?.nativeElement)
        this.chatBody.nativeElement.scrollTop =
          this.chatBody.nativeElement.scrollHeight;
    } catch {}
  }
}