import { 
  Component,
  AfterViewInit,
  ElementRef,
  QueryList,
  ViewChildren 
} from '@angular/core';

@Component({
  selector: 'app-education',
  imports: [],
  templateUrl: './education.html',
  styleUrl: './education.css',
})
export class Education implements AfterViewInit {

  @ViewChildren('eduCard') cards!: QueryList<ElementRef>;
education = [
    {
      degree: 'Bachelor of Technology (B.Tech)',
      institution: 'Uttarakhand Technical University',
      period: '2007 – 2011',
      score: '75.6%'
    },
    {
      degree: 'Senior Secondary (XII)',
      institution: 'CBSE Board',
      period: '2007',
      score: '77.8%'
    },
    {
      degree: 'Secondary (X)',
      institution: 'CBSE Board',
      period: '2005',
      score: '80.4%'
    }
  ];

  certifications = [
    {
      title: 'Infosys Entry Level Training – Microsoft Track',
      description: 'Completed with CGPA 4.93 out of 5'
    }
  ];
  ngAfterViewInit() {
    const observer = new IntersectionObserver(
      entries => {
        entries.forEach(entry => {
          if (entry.isIntersecting) {
            entry.target.classList.add('visible');
            observer.unobserve(entry.target);
          }
        });
      },
      { threshold: 0.2 }
    );

    this.cards.forEach(card => observer.observe(card.nativeElement));
  }
}
