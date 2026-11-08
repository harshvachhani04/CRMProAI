import { Component, input } from '@angular/core';
import { RouterLink } from '@angular/router';

@Component({
  selector: 'app-error-page',
  standalone: true,
  imports: [RouterLink],
  template: `
    <div class="error-container card-surface">
      <div class="icon-bubble">⚠</div>
      <h2>{{ title() }}</h2>
      <p>{{ message() }}</p>

      @if (traceId()) {
        <div class="trace-box">
          <small>Trace ID for support:</small>
          <code>{{ traceId() }}</code>
        </div>
      }

      <div class="actions">
        <a routerLink="/dashboard" class="primary-btn">
          <span>⌂</span> Return to Dashboard
        </a>
      </div>
    </div>
  `,
  styles: [`
    .error-container {
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
      background: #fee2e2;
      color: #ef4444;
      display: grid;
      place-items: center;
      font-size: 28px;
      font-weight: 800;
      margin-bottom: 20px;
    }
    h2 {
      margin: 0 0 10px;
      font-size: 22px;
      color: #1e293b;
    }
    p {
      margin: 0 0 20px;
      font-size: 13px;
      color: #64738e;
      line-height: 1.5;
    }
    .trace-box {
      background: #f8fafc;
      border: 1px solid #e2e8f0;
      padding: 8px 14px;
      border-radius: 8px;
      margin-bottom: 24px;
      display: flex;
      flex-direction: column;
      gap: 4px;
    }
    .trace-box small {
      font-size: 11px;
      color: #94a3b8;
    }
    .trace-box code {
      font-size: 12px;
      font-weight: 600;
      color: #334155;
    }
    .actions {
      display: flex;
      gap: 12px;
    }
  `]
})
export class ErrorPageComponent {
  readonly title = input<string>('Unexpected Error');
  readonly message = input<string>('An error occurred while processing your request. Please quote your trace ID if contacting support.');
  readonly traceId = input<string | null>(null);
}
