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
        data: { breadcrumb: 'Overview' },
        loadComponent: () => import('./features/dashboard/dashboard.component').then(m => m.DashboardComponent)
      },
      {
        path: 'leads',
        data: { breadcrumb: 'Leads' },
        loadComponent: () => import('./features/leads/leads-list.component').then(m => m.LeadsListComponent)
      },
      {
        path: 'accounts',
        data: { breadcrumb: 'Accounts' },
        loadComponent: () => import('./features/accounts/accounts-list.component').then(m => m.AccountsListComponent)
      },
      {
        path: 'contacts',
        data: { breadcrumb: 'Contacts' },
        loadComponent: () => import('./features/contacts/contacts-list.component').then(m => m.ContactsListComponent)
      },
      {
        path: 'opportunities',
        data: { breadcrumb: 'Opportunities' },
        loadComponent: () => import('./features/opportunities/opportunities-list.component').then(m => m.OpportunitiesListComponent)
      },
      {
        path: 'activities',
        data: { breadcrumb: 'Activities' },
        loadComponent: () => import('./features/activities/activities-list.component').then(m => m.ActivitiesListComponent)
      },
      {
        path: 'tickets',
        data: { breadcrumb: 'Support Tickets' },
        loadComponent: () => import('./features/tickets/tickets-list.component').then(m => m.TicketsListComponent)
      },
      {
        path: 'knowledge',
        data: { breadcrumb: 'Knowledge Base' },
        loadComponent: () => import('./features/knowledge/knowledge-list.component').then(m => m.KnowledgeListComponent)
      },
      {
        path: 'ai-assistant',
        data: { breadcrumb: 'AI Assistant' },
        loadComponent: () => import('./features/ai-assistant/ai-assistant.component').then(m => m.AiAssistantComponent)
      },
      {
        path: 'reports',
        data: { breadcrumb: 'Reports' },
        loadComponent: () => import('./features/reports/reports-list.component').then(m => m.ReportsListComponent)
      },
      {
        path: 'settings',
        data: { breadcrumb: 'Settings' },
        loadComponent: () => import('./features/settings/settings.component').then(m => m.SettingsComponent)
      },
      {
        path: 'not-found',
        data: { breadcrumb: 'Not Found' },
        loadComponent: () => import('./core/components/not-found/not-found.component').then(m => m.NotFoundComponent)
      }
    ]
  },
  {
    path: '**',
    redirectTo: 'not-found'
  }
];
