import { Component } from '@angular/core';
import { RouterLink } from '@angular/router';

@Component({
  selector: 'app-not-found',
  standalone: true,
  imports: [RouterLink],
  template: `
    <div class="not-found-container card-surface">
      <div class="icon-bubble">404</div>
      <h2>Page Not Found</h2>
      <p>The page or record you are looking for does not exist or has been moved.</p>
      <div class="actions">
        <a routerLink="/dashboard" class="primary-btn">
          <span>⌂</span> Return to Dashboard
        </a>
      </div>
    </div>
  `,
  styles: [`
    .not-found-container {
      display: flex;
      flex-direction: column;
      align-items: center;
      justify-content: center;
      text-align: center;
      padding: 60px 24px;
      margin: 40px auto;
      max-width: 520px;
      border-radius: 16px;
    }
    .icon-bubble {
      width: 72px;
      height: 72px;
      border-radius: 50%;
      background: #f0efff;
      color: var(--purple);
      display: grid;
      place-items: center;
      font-size: 20px;
      font-weight: 800;
      margin-bottom: 20px;
    }
    h2 {
      margin: 0 0 10px;
      font-size: 22px;
      color: #1e293b;
    }
    p {
      margin: 0 0 24px;
      font-size: 13px;
      color: #64738e;
      line-height: 1.5;
    }
    .actions {
      display: flex;
      gap: 12px;
    }
  `]
})
export class NotFoundComponent {}
