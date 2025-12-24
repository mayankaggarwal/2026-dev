import { 
  Component,
  AfterViewInit,
  ElementRef,
  QueryList,
  ViewChildren 
} from '@angular/core';

@Component({
  standalone: true,
  selector: 'app-achievements',
  imports: [],
  templateUrl: './achievements.html',
  styleUrl: './achievements.css',
})
export class Achievements implements AfterViewInit {

  @ViewChildren('achievementCard') cards!: QueryList<ElementRef>;
achievements = [
    {
      icon: '🏆',
      title: 'Guinness World Record',
      description:
        'Record holder for “Most participants in an App Development Marathon” at Windows AppFest 2012.'
    },
    {
      icon: '📄',
      title: 'International White Paper',
      description:
        'White paper published at SRII 2014 Conference, San Jose.'
    },
    {
      icon: '📱',
      title: 'Windows Store App',
      description:
        'Memory Champ app selected for Windows Store at Windows AppFest 2012.'
    },
    {
      icon: '🎓',
      title: 'Graduated with Honors',
      description:
        'Completed graduation with Honors and strong academic performance.'
    },
    {
      icon: '🏅',
      title: 'Infosys Training Excellence',
      description:
        'Completed Infosys Microsoft track training with CGPA 4.93/5.'
    },
    {
      icon: '🎨',
      title: 'National Art Award',
      description:
        'Winner of “CHITRAKALA – Kalabhoshan” award in national competition.'
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
