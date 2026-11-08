import { Component } from '@angular/core';

@Component({
  selector: 'app-leads-list',
  standalone: true,
  template: `
    <div class="page-container">
      <div class="page-header">
        <div>
          <small class="eyebrow">PIPELINE INTAKE</small>
          <h1 class="page-title">Lead Management</h1>
          <p class="page-subtitle">Capture, qualify, assign, and convert incoming IT services enquiries.</p>
        </div>
        <div class="header-actions">
          <button class="secondary-btn">⇩ Export Excel</button>
          <button class="primary-btn">＋ Add Lead</button>
        </div>
      </div>

      <div class="card-surface table-card">
        <div class="table-toolbar">
          <div class="search-box">
            <span>⌕</span>
            <input type="text" placeholder="Search leads by name, email, company..." />
          </div>
          <button class="secondary-btn">Filters</button>
        </div>
        <div class="empty-state">
          <div class="empty-icon">◎</div>
          <h3>Lead Registry Ready</h3>
          <p>Leads captured through APIs and forms will appear here in real time.</p>
        </div>
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
    .table-card { padding: 0; overflow: hidden; }
    .table-toolbar { display: flex; justify-content: space-between; padding: 16px 20px; border-bottom: 1px solid var(--line); }
    .search-box {
      display: flex;
      align-items: center;
      gap: 8px;
      border: 1px solid #e2e8f0;
      border-radius: 8px;
      padding: 6px 12px;
      width: 320px;
      background: #f8fafc;
    }
    .search-box input { border: 0; outline: 0; background: transparent; font-size: 12px; width: 100%; }
    .empty-state { text-align: center; padding: 60px 20px; color: #64738e; }
    .empty-icon { font-size: 32px; color: var(--purple); margin-bottom: 12px; }
    .empty-state h3 { font-family: 'Manrope', sans-serif; font-size: 16px; margin: 0 0 6px; color: #1e293b; }
    .empty-state p { font-size: 13px; margin: 0; }
  `]
})
export class LeadsListComponent {}
