import { Component, HostListener, inject, signal } from '@angular/core';
import { NavigationEnd, Router, RouterOutlet } from '@angular/router';
import { Header } from './layout/header/header';
import { Footer } from './layout/footer/footer';
import { ChatService } from './core/chat';
import { Chat } from './features/chat/chat';
import { filter } from 'rxjs/operators';
import { AnalyticsService } from './core/analytics.service';

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
   router = inject(Router);
  chatService = inject(ChatService);
  analytics = inject(AnalyticsService);

  constructor() {
    this.router.events
  .pipe(
    filter(event => event instanceof NavigationEnd)
  )
  .subscribe((event: any) => {

    this.analytics.trackPageView(
      event.urlAfterRedirects
    );

  });
  }
}
