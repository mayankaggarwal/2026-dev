import { 
  Component,
  AfterViewInit,
  ElementRef,
  QueryList,
  ViewChildren 
} from '@angular/core';

@Component({
  selector: 'app-skills',
  imports: [],
  templateUrl: './skills.html',
  styleUrl: './skills.css',
})
export class Skills implements AfterViewInit {

  @ViewChildren('skillBadge') badges!: QueryList<ElementRef>;
skills = {
  frontend: [
    'Angular (5 – 15+)',
    'TypeScript',
    'JavaScript',
    'HTML5',
    'CSS3'
  ],
  backend: [
    '.NET Core / .NET 6',
    'ASP.NET Web API',
    'gRPC',
    'SignalR',
    'WCF'
  ],
  cloudDevOps: [
    'AWS (EKS, ECS, Fargate, DocumentDB)',
    'Azure (Functions, Service Bus, WebApps)',
    'Docker',
    'Kubernetes',
    'Helm Charts',
    'CI/CD Pipelines (GitLab, Azure DevOps)'
  ],
  databases: [
    'PostgreSQL',
    'MS SQL Server',
    'MySQL',
    'Couchbase',
    'Aerospike',
    'ElasticSearch'
  ],
  messaging: [
    'RabbitMQ',
    'Azure Service Bus'
  ],
  tools: [
    'Visual Studio',
    'VS Code',
    'Git',
    'GitLab',
    'Azure DevOps'
  ]
};
ngAfterViewInit() {
    this.badges.forEach((badge, index) => {
      setTimeout(() => {
        badge.nativeElement.classList.add('visible');
      }, index * 60);
    });
  }
}
