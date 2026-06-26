import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import {
  MessageResponse, SendMessageRequest, ConversationSummary
} from '../models/message.models';

@Injectable({ providedIn: 'root' })
export class MessageService {
  private http    = inject(HttpClient);
  private baseUrl = `${environment.apiUrl}/messages`;

  sendMessage(dto: SendMessageRequest): Observable<MessageResponse> {
    return this.http.post<MessageResponse>(this.baseUrl, dto);
  }

  getInbox(): Observable<MessageResponse[]> {
    return this.http.get<MessageResponse[]>(`${this.baseUrl}/inbox`);
  }

  getSent(): Observable<MessageResponse[]> {
    return this.http.get<MessageResponse[]>(`${this.baseUrl}/sent`);
  }

  getConversations(): Observable<ConversationSummary[]> {
    return this.http.get<ConversationSummary[]>(`${this.baseUrl}/conversations`);
  }

  getConversation(otherUserId: string): Observable<MessageResponse[]> {
    return this.http.get<MessageResponse[]>(
      `${this.baseUrl}/conversation/${otherUserId}`
    );
  }

  getThread(parentMessageId: string): Observable<MessageResponse[]> {
    return this.http.get<MessageResponse[]>(
      `${this.baseUrl}/thread/${parentMessageId}`
    );
  }

  markAsRead(messageId: string): Observable<any> {
    return this.http.put(`${this.baseUrl}/${messageId}/read`, {});
  }

  getUnreadCount(): Observable<{ count: number }> {
    return this.http.get<{ count: number }>(`${this.baseUrl}/unread-count`);
  }

  delete(messageId: string): Observable<any> {
    return this.http.delete(`${this.baseUrl}/${messageId}`);
  }
}