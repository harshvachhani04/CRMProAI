import { Component } from '@angular/core';

@Component({
  selector: 'app-knowledge-list',
  standalone: true,
  template: `
    <div class="page-container">
      <div class="page-header">
        <div>
          <small class="eyebrow">RAG KNOWLEDGE BASE</small>
          <h1 class="page-title">Articles & Ingested Documents</h1>
          <p class="page-subtitle">Approved company SOPs, manuals, and documents for hybrid semantic retrieval.</p>
        </div>
        <div class="header-actions">
          <button class="secondary-btn">Upload Document</button>
          <button class="primary-btn">＋ New Article</button>
        </div>
      </div>

      <div class="card-surface empty-card">
        <div class="empty-icon">▧</div>
        <h3>Knowledge Base Ready</h3>
        <p>Published articles and pgvector document embeddings will be managed here.</p>
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
    .empty-icon { font-size: 32px; color: var(--green); margin-bottom: 12px; }
    .empty-card h3 { font-family: 'Manrope', sans-serif; font-size: 16px; margin: 0 0 6px; color: #1e293b; }
    .empty-card p { font-size: 13px; margin: 0; }
  `]
})
export class KnowledgeListComponent {}
