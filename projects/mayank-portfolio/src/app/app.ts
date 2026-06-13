import { Component, HostListener, inject, signal } from '@angular/core';
import { NavigationEnd, Router, RouterOutlet } from '@angular/router';
import { Header } from './layout/header/header';
import { Footer } from './layout/footer/footer';
import { ChatService } from './core/chat';
import { Chat } from './features/chat/chat';
import { filter } from 'rxjs/operators';

@Component({
  selector: 'app-root',
  imports: [
    RouterOutlet,
    Header,
    Footer,
    Chat
  ],
  templateUrl: './app.html',
  styleUrl: './app.css'
})
export class App {
  protected readonly title = signal('mayank-portfolio');
  chatService = inject(ChatService);
  router = inject(Router);

    constructor() {

    this.router.events
      .pipe(
        filter(event => event instanceof NavigationEnd)
      )
      .subscribe(() => {

        if (this.router.url !== '/chat') {
          this.chatService.lastRoute = this.router.url;
        }

      });

    this.syncChatMode();
  }
  @HostListener('window:resize')
  onResize() {
    this.syncChatMode();
  }

  private syncChatMode() {

    const mobile = window.innerWidth <= 768;

    if (mobile) {

      // Desktop panel should never remain open
      this.chatService.close();

    } else {

      // Desktop mode should never remain on chat route
      if (this.router.url === '/chat') {

        this.router.navigateByUrl(
          this.chatService.lastRoute || '/'
        );

      }

    }
  }
}
