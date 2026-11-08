import { Component } from '@angular/core';

@Component({
  selector: 'app-contacts-list',
  standalone: true,
  template: `
    <div class="page-container">
      <div class="page-header">
        <div>
          <small class="eyebrow">STAKEHOLDERS</small>
          <h1 class="page-title">Contacts Directory</h1>
          <p class="page-subtitle">Track decision-makers, technical evaluators, and billing contacts.</p>
        </div>
        <div class="header-actions">
          <button class="secondary-btn">⇩ Export Excel</button>
          <button class="primary-btn">＋ Add Contact</button>
        </div>
      </div>

      <div class="card-surface empty-card">
        <div class="empty-icon">♙</div>
        <h3>Contacts Directory Ready</h3>
        <p>Stakeholder contacts mapped to accounts will appear here.</p>
      </div>
    </div>
  `,
  styles: [`
    .page-container { display: flex; flex-direction: column; gap: 24px; }
    .page-header { display: flex; justify-content: space-between; align-items: flex-end; }
    .eyebrow { font-size: 10px; font-weight: 700; letter-spacing: 1.2px; color: var(--purple); }
    .page-title { font-family: 'Manrope', sans-serif; font-size: 24px; font-weight: 800; margin: 6px 0; color: #1e293b; }
    .page-subtitle { color: #64738e; font-size: 13px; margin: 0; }
    .header-actions { display: flex; gap: 10px; }
    .empty-card { text-align: center; padding: 60px 20px; color: #64738e; }
    .empty-icon { font-size: 32px; color: var(--purple); margin-bottom: 12px; }
    .empty-card h3 { font-family: 'Manrope', sans-serif; font-size: 16px; margin: 0 0 6px; color: #1e293b; }
    .empty-card p { font-size: 13px; margin: 0; }
  `]
})
export class ContactsListComponent {}
