import { Routes } from '@angular/router';

export const routes: Routes = [
    {
    path: '',
    loadComponent: () =>
      import('./features/about/about')
        .then(m => m.About)
    },
    {
        path: 'achievements',
        loadComponent: () =>
        import('./features/achievements/achievements')
            .then(m => m.Achievements)
    },{
        path: 'education',
        loadComponent: () =>
            import('./features/education/education')
            .then(m => m.Education)
        },
    {
        path: 'experience',
        loadComponent: () =>
        import('./features/experience/experience')
            .then(m => m.Experience)
    },
    {
        path: 'projects',
        loadComponent: () =>
        import('./features/projects/projects')
            .then(m => m.Projects)
    },
    {
        path: 'skills',
        loadComponent: () =>
        import('./features/skills/skills')
            .then(m => m.Skills)
    },
    { path: '**', redirectTo: '' }
];
