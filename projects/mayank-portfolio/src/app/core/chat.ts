import { Injectable, signal } from '@angular/core';
declare let gtag: Function;
@Injectable({
  providedIn: 'root',
})
export class ChatService {
    isOpen = signal(false);
    chatOpenedAt: number = Date.now();
  open() {
      this.isOpen.set(true);
      this.chatOpenedAt = Date.now();
      gtag(
      'event',
      'chat_opened',
      {
        source: 'portfolio'
      }
    );
  }

  close() {
    this.isOpen.set(false);
    const seconds =
    Math.floor(
      (Date.now() - this.chatOpenedAt ) / 1000
    );

    gtag(
      'event',
      'chat_duration',
      {
        duration_seconds: seconds
      }
    );
  }

  toggle() {
    this.isOpen.update(v => !v);
  }
}
