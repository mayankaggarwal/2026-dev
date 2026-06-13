import { Component, inject } from '@angular/core';
import { ChatService } from '../../core/chat';

@Component({
  selector: 'app-chat',
  imports: [],
  templateUrl: './chat.html',
  styleUrl: './chat.css',
  standalone: true
})
export class Chat {
  chatUrl = 'https://mayankaggarwal9-career-conversations.hf.space';
    private chatService = inject(ChatService);

  close() {
    this.chatService.close();
  }
}
