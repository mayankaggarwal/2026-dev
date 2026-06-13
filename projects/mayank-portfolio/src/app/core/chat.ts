import { Injectable, signal } from '@angular/core';

@Injectable({
  providedIn: 'root',
})
export class ChatService {
    isOpen = signal(false);
  lastRoute = '/';
  open() {
    this.isOpen.set(true);
  }

  close() {
    this.isOpen.set(false);
  }

  toggle() {
    this.isOpen.update(v => !v);
  }
}
