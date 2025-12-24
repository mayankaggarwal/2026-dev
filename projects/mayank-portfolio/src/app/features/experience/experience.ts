import {   
  Component,
  AfterViewInit,
  ElementRef,
  QueryList,
  ViewChildren,
  ViewChild 
} from '@angular/core';

@Component({
  selector: 'app-experience',
  imports: [],
  templateUrl: './experience.html',
  styleUrl: './experience.css',
})
export class Experience implements AfterViewInit  {
  @ViewChildren('timelineItem') items!: QueryList<ElementRef>;
  @ViewChild('timeline') timeline!: ElementRef;
experience = [
  {
    role: 'Lead II',
    company: 'S&P Global',
    period: 'June 2023 – Present',
    highlights: [
      'Leading a team delivering large-scale modernization and migration initiatives.',
      'Migrated legacy ActiveX-based UI to Angular 15.',
      'Integrated Web APIs with OIDC providers and exposed secure public APIs.',
      'Migrated on-prem infrastructure to AWS using Lift-and-Shift strategy.'
    ]
  },
  {
    role: 'Software Development Engineer III',
    company: 'ShipBob',
    period: 'July 2022 – June 2023',
    highlights: [
      'Developed distributed systems for global shipment and carrier integrations.',
      'Improved system reliability using circuit breaker patterns.',
      'Enhanced observability using Azure Application Insights.',
      'Optimized CI/CD pipelines to reduce deployment and delivery time.'
    ]
  },
  {
    role: 'Senior Software Engineer',
    company: 'IHS Markit',
    period: 'April 2020 – July 2022',
    highlights: [
      'Built shared platform services for identity, tenant, and configuration management.',
      'Implemented RBAC using Casbin.',
      'Integrated SSO with IdentityServer4, ADFS, and Okta.',
      'Leveraged Kubernetes-native features like secrets, certificates, and health checks.'
    ]
  },
  {
    role: 'Lead Developer',
    company: 'Dunnhumby',
    period: 'April 2017 – April 2020',
    highlights: [
      'Developed Angular-based frontend applications and .NET Core backend services.',
      'Migrated legacy applications to modern tech stacks.',
      'Delivered campaign management and product recommendation platforms.'
    ]
  },
  {
    role: 'System Specialist',
    company: 'Orange Business Services',
    period: 'December 2014 – April 2017',
    highlights: [
      'Designed and developed enterprise data warehouse solutions.',
      'Led a team providing architectural and technical guidance.',
      'Maintained and enhanced existing production systems.'
    ]
  },
  {
    role: 'Senior Systems Engineer',
    company: 'Infosys Limited',
    period: 'February 2012 – October 2014',
    highlights: [
      'Developed enterprise web applications using ASP.NET and WCF.',
      'Worked on OCR automation, ETL pipelines, and analytics platforms.',
      'Contributed to Microsoft Technology Center (MTC) research projects.'
    ]
  }
];

ngAfterViewInit() {
    /* Timeline line observer */
    const lineObserver = new IntersectionObserver(
      ([entry]) => {
        if (entry.isIntersecting) {
          this.timeline.nativeElement.classList.add('draw');
          lineObserver.disconnect();
        }
      },
      { threshold: 0.2 }
    );

    lineObserver.observe(this.timeline.nativeElement);

    /* Timeline item observer */
    const itemObserver = new IntersectionObserver(
      entries => {
        entries.forEach(entry => {
          if (entry.isIntersecting) {
            entry.target.classList.add('visible');
            itemObserver.unobserve(entry.target);
          }
        });
      },
      { threshold: 0.2 }
    );

    this.items.forEach(item => itemObserver.observe(item.nativeElement));
  }

}
