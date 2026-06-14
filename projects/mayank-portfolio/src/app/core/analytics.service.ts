import { Injectable } from '@angular/core';
declare let gtag: Function;
@Injectable({
  providedIn: 'root',
})
export class AnalyticsService {
    trackPageView(url: string) {

    gtag(
      'config',
      'G-QJP5LXK84W',
      {
        page_path: url
      }
    );

  }
}
