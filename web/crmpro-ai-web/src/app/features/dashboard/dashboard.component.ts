import { Component } from '@angular/core';

@Component({
  selector: 'app-dashboard',
  standalone: true,
  template: `
    <div class="dashboard-container">
      <div class="page-header">
        <div>
          <small class="eyebrow">EXECUTIVE WORKSPACE</small>
          <h1 class="page-title">Executive Overview <span>✦</span></h1>
          <p class="page-subtitle">Real-time visibility across pipeline, deals, and support SLA performance.</p>
        </div>
        <div class="header-actions">
          <button class="secondary-btn">⇩ Export Overview</button>
          <button class="primary-btn">＋ New Lead</button>
        </div>
      </div>

      <!-- Metric Cards Grid -->
      <div class="metrics-grid">
        <div class="metric-card">
          <div class="metric-header">
            <span>Total Pipeline</span>
            <span class="metric-icon purple">◇</span>
          </div>
          <strong class="metric-value">$284,500</strong>
          <small class="metric-trend up">↗ 12.8% vs last month</small>
        </div>

        <div class="metric-card">
          <div class="metric-header">
            <span>Active Deals</span>
            <span class="metric-icon blue">◎</span>
          </div>
          <strong class="metric-value">48</strong>
          <small class="metric-trend up">↗ 8.2% vs last month</small>
        </div>

        <div class="metric-card">
          <div class="metric-header">
            <span>Won (Month)</span>
            <span class="metric-icon green">✓</span>
          </div>
          <strong class="metric-value">$68,200</strong>
          <small class="metric-trend up">↗ 18.4% vs last month</small>
        </div>

        <div class="metric-card">
          <div class="metric-header">
            <span>Open Tickets</span>
            <span class="metric-icon orange">◷</span>
          </div>
          <strong class="metric-value">16</strong>
          <small class="metric-trend down">↘ 4.3% vs last month</small>
        </div>
      </div>

      <!-- AI Insight Banner -->
      <div class="ai-banner">
        <div class="ai-icon">✦</div>
        <div class="ai-copy">
          <small>CRMPRO AI INTELLIGENCE</small>
          <h3>3 opportunities may require attention</h3>
          <p>Deals in Proposal stage have exceeded 14 days without active follow-ups.</p>
        </div>
        <button class="primary-btn">Review with AI →</button>
      </div>
    </div>
  `,
  styles: [`
    .dashboard-container { display: flex; flex-direction: column; gap: 24px; }
    .page-header { display: flex; justify-content: space-between; align-items: flex-end; }
    .eyebrow { font-size: 10px; font-weight: 700; letter-spacing: 1.2px; color: var(--purple); }
    .page-title { font-family: 'Manrope', sans-serif; font-size: 26px; font-weight: 800; margin: 6px 0; color: #1e293b; }
    .page-title span { color: #f59e0b; font-size: 18px; }
    .page-subtitle { color: #64738e; font-size: 13px; margin: 0; }
    .header-actions { display: flex; gap: 10px; }

    .metrics-grid {
      display: grid;
      grid-template-columns: repeat(auto-fit, minmax(220px, 1fr));
      gap: 16px;
    }
    .metric-card {
      background: #fff;
      border: 1px solid #edf0f5;
      border-radius: 12px;
      padding: 20px;
      box-shadow: 0 4px 20px rgba(25, 39, 70, 0.04);
    }
    .metric-header { display: flex; justify-content: space-between; align-items: center; font-size: 12px; font-weight: 600; color: #64738e; }
    .metric-icon {
      width: 32px;
      height: 32px;
      border-radius: 8px;
      display: grid;
      place-items: center;
      font-size: 14px;
    }
    .metric-icon.purple { background: var(--purple-light); color: var(--purple); }
    .metric-icon.blue { background: var(--blue-light); color: var(--blue); }
    .metric-icon.green { background: var(--green-light); color: var(--green); }
    .metric-icon.orange { background: var(--orange-light); color: var(--orange); }

    .metric-value { display: block; font-family: 'Manrope', sans-serif; font-size: 26px; font-weight: 800; color: #1e293b; margin: 12px 0 6px; }
    .metric-trend { font-size: 11px; font-weight: 600; }
    .metric-trend.up { color: var(--green); }
    .metric-trend.down { color: var(--orange); }

    .ai-banner {
      display: flex;
      align-items: center;
      gap: 16px;
      background: linear-gradient(105deg, #f2f0ff, #f8f7ff 65%, #f0f6ff);
      border: 1px solid #e8e5ff;
      border-radius: 12px;
      padding: 18px 24px;
    }
    .ai-icon {
      width: 40px;
      height: 40px;
      border-radius: 10px;
      background: #fff;
      color: var(--purple);
      display: grid;
      place-items: center;
      font-size: 20px;
      box-shadow: 0 4px 12px rgba(118, 103, 232, 0.15);
    }
    .ai-copy { flex: 1; }
    .ai-copy small { font-size: 9px; font-weight: 800; letter-spacing: 1px; color: var(--purple); }
    .ai-copy h3 { font-family: 'Manrope', sans-serif; font-size: 14px; font-weight: 700; margin: 4px 0; color: #2e3458; }
    .ai-copy p { font-size: 12px; color: #6b7280; margin: 0; }
  `]
})
export class DashboardComponent {}
