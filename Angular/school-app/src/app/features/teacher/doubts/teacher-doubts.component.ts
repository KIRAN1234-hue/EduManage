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
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatSnackBar, MatSnackBarModule } from '@angular/material/snack-bar';
import { Store } from '@ngrx/store';
import { filter, take } from 'rxjs/operators';

import { MessageService } from '../../../core/services/message.service';
import { MessageResponse, ConversationSummary } from '../../../core/models/message.models';
import { selectCurrentUser, selectAccessToken } from '../../../core/store/auth/auth.selectors';

@Component({
  selector: 'app-teacher-doubts',
  standalone: true,
  imports: [
    CommonModule, FormsModule, ReactiveFormsModule,
    MatIconModule, MatButtonModule, MatFormFieldModule,
    MatInputModule, MatProgressSpinnerModule, MatSnackBarModule
  ],
  templateUrl: './teacher-doubts.component.html',
  styleUrls: ['./teacher-doubts.component.scss']
})
export class TeacherDoubtsComponent implements OnInit, AfterViewChecked {
  @ViewChild('chatBody') chatBody!: ElementRef;

  private messageService = inject(MessageService);
  private store          = inject(Store);

  myUserId          = '';
  conversations:     ConversationSummary[] = [];
  activeMessages:    MessageResponse[]     = [];
  selectedConv       = signal<ConversationSummary | null>(null);
  isLoadingConvs     = signal(false);
  isLoadingMessages  = signal(false);
  isSending          = signal(false);
  searchTerm         = '';
  activeFilter       = 'all';

  replyControl = new FormControl('', Validators.required);

  get filteredConversations(): ConversationSummary[] {
    let list = this.conversations;
    if (this.activeFilter === 'unread')
      list = list.filter(c => c.unreadCount > 0);
    if (this.searchTerm)
      list = list.filter(c =>
        c.otherUserName.toLowerCase().includes(this.searchTerm.toLowerCase())
      );
    return list;
  }

  get totalUnread(): number {
    return this.conversations.reduce((s, c) => s + c.unreadCount, 0);
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
}

  ngAfterViewChecked(): void {
    try {
      if (this.chatBody?.nativeElement)
        this.chatBody.nativeElement.scrollTop =
          this.chatBody.nativeElement.scrollHeight;
    } catch {}
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
    this.isLoadingMessages.set(true);

    this.messageService.getConversation(conv.otherUserId).subscribe({
      next: d => {
        this.activeMessages = d;
        conv.unreadCount = 0;
        this.isLoadingMessages.set(false);
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
      },
      error: () => this.isSending.set(false)
    });
  }

  isMyMessage(msg: MessageResponse): boolean {
    if (!this.myUserId) return false;
    return msg.senderId === this.myUserId;
  }

  getAvatarColor(name: string): string {
    const colors = ['#7C3AED','#2563EB','#16A34A','#D97706','#DC2626','#0891B2'];
    return colors[name.charCodeAt(0) % colors.length];
  }

  getInitials(name: string): string {
    return name.split(' ').map(n => n[0]).join('').substring(0, 2).toUpperCase();
  }

  formatTime(dateStr: string): string {
    const d = new Date(dateStr);
    const diff = Math.floor((Date.now() - d.getTime()) / 86400000);
    if (diff === 0) return d.toLocaleTimeString([], { hour:'2-digit', minute:'2-digit' });
    if (diff === 1) return 'Yesterday';
    if (diff < 7)  return d.toLocaleDateString([], { weekday: 'short' });
    return d.toLocaleDateString([], { month: 'short', day: 'numeric' });
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

  formatMessageTime(dateStr: string): string {
    return new Date(dateStr).toLocaleTimeString([], {
      hour: '2-digit', minute: '2-digit'
    });
  }
}