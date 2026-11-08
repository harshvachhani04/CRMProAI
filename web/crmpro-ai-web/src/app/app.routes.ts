import { Routes } from '@angular/router';
import { AppShellComponent } from './layouts/app-shell/app-shell.component';

export const routes: Routes = [
  {
    path: '',
    component: AppShellComponent,
    children: [
      { path: '', redirectTo: 'dashboard', pathMatch: 'full' },
      {
        path: 'dashboard',
        loadComponent: () => import('./features/dashboard/dashboard.component').then(m => m.DashboardComponent)
      },
      {
        path: 'leads',
        loadComponent: () => import('./features/leads/leads-list.component').then(m => m.LeadsListComponent)
      },
      {
        path: 'accounts',
        loadComponent: () => import('./features/accounts/accounts-list.component').then(m => m.AccountsListComponent)
      },
      {
        path: 'contacts',
        loadComponent: () => import('./features/contacts/contacts-list.component').then(m => m.ContactsListComponent)
      },
      {
        path: 'opportunities',
        loadComponent: () => import('./features/opportunities/opportunities-list.component').then(m => m.OpportunitiesListComponent)
      },
      {
        path: 'activities',
        loadComponent: () => import('./features/activities/activities-list.component').then(m => m.ActivitiesListComponent)
      },
      {
        path: 'tickets',
        loadComponent: () => import('./features/tickets/tickets-list.component').then(m => m.TicketsListComponent)
      },
      {
        path: 'knowledge',
        loadComponent: () => import('./features/knowledge/knowledge-list.component').then(m => m.KnowledgeListComponent)
      },
      {
        path: 'ai-assistant',
        loadComponent: () => import('./features/ai-assistant/ai-assistant.component').then(m => m.AiAssistantComponent)
      },
      {
        path: 'reports',
        loadComponent: () => import('./features/reports/reports-list.component').then(m => m.ReportsListComponent)
      },
      {
        path: 'settings',
        loadComponent: () => import('./features/settings/settings.component').then(m => m.SettingsComponent)
      }
    ]
  },
  { path: '**', redirectTo: 'dashboard' }
];
