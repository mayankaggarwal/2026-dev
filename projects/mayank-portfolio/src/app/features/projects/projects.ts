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
    name: 'Primary Markets Platform GenAI Integration',
    description:
      'Enabled development team to use custom AI agents and skills enabled using markdown files to be used with copilot ',
    tech: ['.NET','Copilot', 'Agentic AI', 'MCP']
  },
  {
    name: 'Primary Markets Platform backend Modernization',
    description:
      'Delivered project to migrate 20-year-old legacy C++ codebase to scalable .NET solution where involved in all aspects of the project including planning, architecture design, development, and deployment.',
    tech: ['.NET','Containerization', 'AWS', 'RabbitMQ', 'MS SQL']
  },
  {
    name: 'Primary Markets Platform UI Modernization',
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
