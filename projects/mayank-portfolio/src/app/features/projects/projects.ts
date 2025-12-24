import { 
  Component,
  AfterViewInit,
  ElementRef,
  QueryList,
  ViewChildren 
} from '@angular/core';

@Component({
  selector: 'app-projects',
  imports: [],
  templateUrl: './projects.html',
  styleUrl: './projects.css',
})
export class Projects implements AfterViewInit {

  @ViewChildren('projectCard') cards!: QueryList<ElementRef>;
projects = [
  {
    name: 'Primary Markets Platform Modernization',
    description:
      'Modernized a legacy financial platform by migrating ActiveX-based UI to Angular and moving on-prem infrastructure to AWS.',
    tech: ['Angular', '.NET', 'AWS', 'OIDC']
  },
  {
    name: 'Shipment Label Purchasing System',
    description:
      'Built microservices for purchasing shipment labels by integrating with multiple carrier partners.',
    tech: ['.NET Core', 'Azure Functions', 'Azure Service Bus', 'MS SQL']
  },
  {
    name: 'Enterprise Identity & RBAC Platform',
    description:
      'Developed shared identity, authentication, and authorization services used across multiple teams.',
    tech: ['Angular', '.NET Core', 'Kubernetes', 'Casbin', 'IdentityServer4']
  },
  {
    name: 'Campaign Manager Portal',
    description:
      'Developed an Angular-based frontend portal for managing campaigns and analytics.',
    tech: ['Angular', 'ASP.NET Core', 'ElasticSearch', 'Docker']
  },
  {
    name: 'Product Recommendation System',
    description:
      'Maintained and deployed a recommendation system for new markets.',
    tech: ['ASP.NET Web API', 'PostgreSQL', 'gRPC']
  },
  {
    name: 'Enterprise Data Warehouse & ETL',
    description:
      'Designed and implemented ETL pipelines and data warehouse architecture for HR analytics.',
    tech: ['MySQL', 'Talend', 'Pentaho', 'Cognos']
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
      { threshold: 0.15 }
    );

    this.cards.forEach(card => observer.observe(card.nativeElement));
  }
}
