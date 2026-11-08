import { Component, signal } from '@angular/core';
import { RouterLink, RouterLinkActive, RouterOutlet } from '@angular/router';

@Component({
  selector: 'app-shell',
  standalone: true,
  imports: [RouterOutlet, RouterLink, RouterLinkActive],
  template: `
    <div class="shell-container">
      <!-- Sidebar -->
      <aside class="sidebar" [class.open]="sidebarOpen()">
        <div class="sidebar-header">
          <div class="logo">
            <span class="logo-icon">✦</span>
            <span>CRMPro <b>AI</b></span>
          </div>
          <button class="close-btn" (click)="toggleSidebar()">×</button>
        </div>

        <div class="workspace-badge">
          <div class="workspace-avatar">N</div>
          <div class="workspace-meta">
            <b>Northstar Digital</b>
            <small>Growth workspace</small>
          </div>
          <span class="workspace-chevron">⌄</span>
        </div>

        <div class="nav-section-title">WORKSPACE</div>
        <nav class="nav-group">
          <a routerLink="/dashboard" routerLinkActive="active" class="nav-item">
            <span class="nav-icon">▦</span>
            <span>Overview</span>
          </a>
          <a routerLink="/leads" routerLinkActive="active" class="nav-item">
            <span class="nav-icon">◎</span>
            <span>Leads</span>
            <span class="nav-pill">12</span>
          </a>
          <a routerLink="/accounts" routerLinkActive="active" class="nav-item">
            <span class="nav-icon">▣</span>
            <span>Accounts</span>
          </a>
          <a routerLink="/contacts" routerLinkActive="active" class="nav-item">
            <span class="nav-icon">♙</span>
            <span>Contacts</span>
          </a>
          <a routerLink="/opportunities" routerLinkActive="active" class="nav-item">
            <span class="nav-icon">◇</span>
            <span>Opportunities</span>
          </a>
          <a routerLink="/activities" routerLinkActive="active" class="nav-item">
            <span class="nav-icon">◷</span>
            <span>Activities</span>
          </a>
        </nav>

        <div class="nav-section-title">CUSTOMER EXPERIENCE</div>
        <nav class="nav-group">
          <a routerLink="/tickets" routerLinkActive="active" class="nav-item">
            <span class="nav-icon">▤</span>
            <span>Support tickets</span>
            <span class="nav-pill amber">4</span>
          </a>
          <a routerLink="/knowledge" routerLinkActive="active" class="nav-item">
            <span class="nav-icon">▧</span>
            <span>Knowledge base</span>
          </a>
        </nav>

        <div class="nav-section-title">INTELLIGENCE</div>
        <nav class="nav-group">
          <a routerLink="/ai-assistant" routerLinkActive="active" class="nav-item ai-nav">
            <span class="nav-icon">✧</span>
            <span>AI Assistant</span>
            <span class="nav-pill new">NEW</span>
          </a>
          <a routerLink="/reports" routerLinkActive="active" class="nav-item">
            <span class="nav-icon">▥</span>
            <span>Reports & analytics</span>
          </a>
          <a routerLink="/settings" routerLinkActive="active" class="nav-item">
            <span class="nav-icon">⚙</span>
            <span>Settings</span>
          </a>
        </nav>

        <div class="sidebar-footer">
          <div class="user-profile">
            <div class="avatar">AM</div>
            <div class="user-meta">
              <b>Alex Morgan</b>
              <small>Sales Manager</small>
            </div>
            <button class="logout-btn" title="Sign out">↪</button>
          </div>
        </div>
      </aside>

      <!-- Main Layout -->
      <div class="main-content">
        <header class="topbar">
          <div class="topbar-left">
            <button class="menu-toggle" (click)="toggleSidebar()">☰</button>
            <div class="breadcrumbs">
              <span>Workspace</span>
              <span class="separator">/</span>
              <b class="current-crumb">CRMPro Operations</b>
            </div>
          </div>

          <div class="topbar-right">
            <div class="global-search">
              <span class="search-icon">⌕</span>
              <input type="text" placeholder="Search leads, accounts, deals..." />
              <kbd>⌘ K</kbd>
            </div>
            <button class="topbar-icon" title="Notifications">🔔</button>
            <div class="user-badge">
              <div class="avatar-small">AM</div>
              <span>Alex Morgan</span>
            </div>
          </div>
        </header>

        <main class="page-body">
          <router-outlet></router-outlet>
        </main>

        <footer class="shell-footer">
          <span>CRMPro AI · Multi-Tenant Enterprise CRM</span>
          <span class="status-indicator">● All systems operational</span>
        </footer>
      </div>
    </div>
  `,
  styles: [`
    .shell-container {
      display: flex;
      min-height: 100vh;
      background: var(--bg);
    }
    .sidebar {
      width: 250px;
      background: var(--navy);
      color: #aab5ca;
      display: flex;
      flex-direction: column;
      position: fixed;
      top: 0;
      bottom: 0;
      left: 0;
      z-index: 50;
      padding: 20px 14px 14px;
      overflow-y: auto;
      transition: transform 0.25s ease;
    }
    .sidebar-header {
      display: flex;
      justify-content: space-between;
      align-items: center;
      margin-bottom: 24px;
      padding: 0 8px;
    }
    .logo {
      font-family: 'Manrope', sans-serif;
      font-weight: 800;
      font-size: 19px;
      color: #fff;
      display: flex;
      align-items: center;
      gap: 8px;
    }
    .logo-icon {
      background: linear-gradient(140deg, #968aff, #6254d5);
      width: 30px;
      height: 30px;
      border-radius: 8px;
      display: grid;
      place-items: center;
      color: #fff;
      font-size: 14px;
    }
    .logo b { color: #a69dff; }
    .close-btn {
      display: none;
      background: none;
      border: 0;
      color: #fff;
      font-size: 24px;
    }
    .workspace-badge {
      display: flex;
      align-items: center;
      gap: 10px;
      padding: 10px;
      background: rgba(255, 255, 255, 0.05);
      border: 1px solid rgba(255, 255, 255, 0.08);
      border-radius: 9px;
      margin-bottom: 22px;
    }
    .workspace-avatar {
      width: 32px;
      height: 32px;
      border-radius: 8px;
      background: linear-gradient(140deg, #65b8d9, #427cbd);
      display: grid;
      place-items: center;
      color: white;
      font-weight: 800;
      font-size: 13px;
    }
    .workspace-meta {
      display: flex;
      flex-direction: column;
      gap: 2px;
      flex: 1;
    }
    .workspace-meta b { font-size: 12px; color: #f1f4fb; }
    .workspace-meta small { font-size: 10px; color: #8290aa; }
    .workspace-chevron { color: #8290aa; font-size: 11px; }

    .nav-section-title {
      font-size: 10px;
      letter-spacing: 1.2px;
      font-weight: 700;
      color: #64738e;
      padding: 0 10px 8px;
      margin-top: 14px;
    }
    .nav-group {
      display: flex;
      flex-direction: column;
      gap: 4px;
    }
    .nav-item {
      height: 38px;
      display: flex;
      align-items: center;
      gap: 10px;
      padding: 0 12px;
      border-radius: 8px;
      color: #9eabc1;
      font-size: 12px;
      font-weight: 500;
      transition: all 0.15s ease;
    }
    .nav-item:hover {
      background: rgba(255, 255, 255, 0.06);
      color: #fff;
    }
    .nav-item.active {
      background: linear-gradient(90deg, rgba(118, 103, 232, 0.25), rgba(118, 103, 232, 0.1));
      color: #fff;
      box-shadow: inset 3px 0 #9b90ff;
      font-weight: 600;
    }
    .nav-icon { font-size: 14px; width: 16px; text-align: center; }
    .nav-pill {
      margin-left: auto;
      background: rgba(255, 255, 255, 0.1);
      padding: 2px 7px;
      border-radius: 12px;
      font-size: 10px;
      font-weight: 700;
    }
    .nav-pill.amber { color: #f4bd75; }
    .nav-pill.new { background: rgba(118, 103, 232, 0.3); color: #c6c1ff; font-size: 9px; }
    .ai-nav { color: #c6c1ff; }

    .sidebar-footer {
      margin-top: auto;
      padding-top: 16px;
      border-top: 1px solid rgba(255, 255, 255, 0.08);
    }
    .user-profile {
      display: flex;
      align-items: center;
      gap: 10px;
      padding: 6px;
    }
    .avatar {
      width: 32px;
      height: 32px;
      border-radius: 50%;
      background: #e9e5ff;
      color: #6557cf;
      display: grid;
      place-items: center;
      font-size: 11px;
      font-weight: 700;
    }
    .user-meta { display: flex; flex-direction: column; gap: 2px; flex: 1; }
    .user-meta b { font-size: 11px; color: #f2f4fa; }
    .user-meta small { font-size: 10px; color: #8290aa; }
    .logout-btn { background: none; border: 0; color: #aab5ca; font-size: 14px; }

    .main-content {
      margin-left: 250px;
      flex: 1;
      display: flex;
      flex-direction: column;
      min-width: 0;
    }
    .topbar {
      height: 64px;
      background: #fff;
      border-bottom: 1px solid var(--line);
      display: flex;
      justify-content: space-between;
      align-items: center;
      padding: 0 32px;
      position: sticky;
      top: 0;
      z-index: 30;
    }
    .topbar-left { display: flex; align-items: center; gap: 14px; }
    .menu-toggle { display: none; background: none; border: 0; font-size: 18px; color: #64738e; }
    .breadcrumbs { font-size: 12px; color: #8993a5; display: flex; gap: 8px; align-items: center; }
    .breadcrumbs .separator { color: #cbd5e1; }
    .breadcrumbs .current-crumb { color: #1e293b; font-weight: 600; }

    .topbar-right { display: flex; align-items: center; gap: 16px; }
    .global-search {
      height: 36px;
      width: 260px;
      border: 1px solid #e2e8f0;
      border-radius: 8px;
      display: flex;
      align-items: center;
      padding: 0 10px;
      gap: 8px;
      background: #f8fafc;
      color: #94a3b8;
    }
    .global-search input {
      border: 0;
      outline: 0;
      background: transparent;
      flex: 1;
      font-size: 12px;
      color: #334155;
    }
    .global-search kbd {
      font-size: 10px;
      border: 1px solid #cbd5e1;
      border-radius: 4px;
      padding: 2px 5px;
      background: #fff;
    }
    .topbar-icon { background: none; border: 0; font-size: 16px; color: #64738e; }
    .user-badge { display: flex; align-items: center; gap: 8px; font-size: 12px; font-weight: 600; color: #334155; }
    .avatar-small {
      width: 28px;
      height: 28px;
      border-radius: 50%;
      background: #f0efff;
      color: var(--purple);
      display: grid;
      place-items: center;
      font-size: 10px;
      font-weight: 700;
    }

    .page-body {
      flex: 1;
      padding: 28px 32px;
      max-width: 1600px;
      width: 100%;
      margin: 0 auto;
    }
    .shell-footer {
      height: 48px;
      background: #fff;
      border-top: 1px solid var(--line);
      padding: 0 32px;
      display: flex;
      justify-content: space-between;
      align-items: center;
      font-size: 11px;
      color: #8993a5;
    }
    .status-indicator { color: var(--green); font-weight: 600; }

    @media (max-width: 900px) {
      .sidebar { transform: translateX(-100%); }
      .sidebar.open { transform: translateX(0); }
      .close-btn { display: block; }
      .main-content { margin-left: 0; }
      .menu-toggle { display: block; }
      .global-search { width: 180px; }
      .page-body { padding: 20px 16px; }
    }
  `]
})
export class AppShellComponent {
  sidebarOpen = signal(false);

  toggleSidebar() {
    this.sidebarOpen.update(v => !v);
  }
}
