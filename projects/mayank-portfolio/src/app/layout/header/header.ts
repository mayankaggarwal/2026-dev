import { Component, signal } from '@angular/core';
import { Router, RouterLink, RouterLinkActive } from '@angular/router';

@Component({
  selector: 'app-header',
  imports: [RouterLink, RouterLinkActive],
  templateUrl: './header.html',
  styleUrl: './header.css',
  standalone: true
})
export class Header {
menuOpen = signal(false);

constructor(private router: Router) {
  this.router.events.subscribe(() => {
    this.menuOpen.set(false);
    document.body.style.overflow = 'auto';
  });
}
  
  toggleMenu(event?: Event) {
  event?.stopPropagation();
      this.menuOpen.update(v => {
    document.body.style.overflow = v ? 'auto' : 'hidden';
    return !v;
  });
  }

  closeMenu() {
      document.body.style.overflow = 'auto';
  this.menuOpen.set(false);
  }
}
