import { Component, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { NotificationService, ToastMessage } from '../../services/notification.service';

@Component({
  selector: 'app-toast-container',
  standalone: true,
  imports: [CommonModule],
  template: `
    <div class="toast-wrapper" aria-live="polite">
      @for (toast of notificationService.messages(); track toast.id) {
        <div class="toast-card" [class]="'toast-' + toast.severity" role="alert">
          <div class="toast-icon">
            @switch (toast.severity) {
              @case ('success') { <span>✓</span> }
              @case ('error') { <span>✕</span> }
              @case ('warn') { <span>!</span> }
              @case ('info') { <span>ℹ</span> }
            }
          </div>
          <div class="toast-content">
            <strong class="toast-summary">{{ toast.summary }}</strong>
            @if (toast.detail) {
              <p class="toast-detail">{{ toast.detail }}</p>
            }
            @if (toast.fieldErrors && toast.fieldErrors.length > 0) {
              <ul class="toast-field-errors">
                @for (err of toast.fieldErrors; track err) {
                  <li>{{ err }}</li>
                }
              </ul>
            }
          </div>
          <button class="toast-close" (click)="dismiss(toast.id)" aria-label="Close notification">×</button>
        </div>
      }
    </div>
  `,
  styles: [`
    .toast-wrapper {
      position: fixed;
      top: 20px;
      right: 20px;
      z-index: 9999;
      display: flex;
      flex-direction: column;
      gap: 10px;
      max-width: 420px;
      width: calc(100% - 40px);
      pointer-events: none;
    }
    .toast-card {
      pointer-events: auto;
      display: flex;
      align-items: flex-start;
      gap: 12px;
      padding: 12px 14px;
      border-radius: 10px;
      background: #ffffff;
      box-shadow: 0 10px 25px rgba(17, 29, 56, 0.15), 0 2px 6px rgba(17, 29, 56, 0.08);
      border-left: 4px solid #cbd5e1;
      animation: slideIn 0.2s cubic-bezier(0.16, 1, 0.3, 1);
    }
    @keyframes slideIn {
      from { transform: translateX(100%); opacity: 0; }
      to { transform: translateX(0); opacity: 1; }
    }
    .toast-success { border-left-color: #25ad7e; }
    .toast-success .toast-icon { background: #e8f8f1; color: #25ad7e; }
    .toast-error { border-left-color: #ef4444; }
    .toast-error .toast-icon { background: #fee2e2; color: #ef4444; }
    .toast-warn { border-left-color: #f59e0b; }
    .toast-warn .toast-icon { background: #fef3c7; color: #f59e0b; }
    .toast-info { border-left-color: #7667e8; }
    .toast-info .toast-icon { background: #f0efff; color: #7667e8; }

    .toast-icon {
      width: 24px;
      height: 24px;
      border-radius: 50%;
      display: grid;
      place-items: center;
      font-size: 12px;
      font-weight: bold;
      flex-shrink: 0;
      margin-top: 2px;
    }
    .toast-content {
      flex: 1;
      min-width: 0;
    }
    .toast-summary {
      display: block;
      font-size: 13px;
      font-weight: 700;
      color: #1e293b;
    }
    .toast-detail {
      margin: 4px 0 0;
      font-size: 12px;
      color: #64738e;
      line-height: 1.4;
      word-break: break-word;
    }
    .toast-field-errors {
      margin: 6px 0 0;
      padding-left: 16px;
      font-size: 11px;
      color: #b91c1c;
      line-height: 1.4;
    }
    .toast-close {
      background: none;
      border: 0;
      color: #94a3b8;
      font-size: 18px;
      line-height: 1;
      padding: 2px 4px;
      cursor: pointer;
      border-radius: 4px;
      transition: color 0.15s;
    }
    .toast-close:hover {
      color: #1e293b;
    }
  `]
})
export class ToastContainerComponent {
  readonly notificationService = inject(NotificationService);

  dismiss(id: string): void {
    this.notificationService.dismiss(id);
  }
}
