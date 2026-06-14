import { Component } from '@angular/core';
declare let gtag: Function;
@Component({
  selector: 'app-footer',
  imports: [],
  templateUrl: './footer.html',
  styleUrl: './footer.css',
  standalone: true
})
export class Footer {
  currentYear: number = new Date().getFullYear();
  trackResumeDownload() {

  gtag(
    'event',
    'resume_download',
    {
      file: 'Mayank_Aggarwal.pdf'
    }
  );

  }

  trackGithubLinkClick() {
    gtag(
      'event',
      'github_link_click',
      {
        source: 'portfolio'
      }
    );
  }
}
