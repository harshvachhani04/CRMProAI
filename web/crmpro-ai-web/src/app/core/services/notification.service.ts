import { Injectable, signal } from '@angular/core';
import { ApiProblemDetails } from '../models/api-response.model';

export type ToastSeverity = 'success' | 'info' | 'warn' | 'error';

export interface ToastMessage {
  id: string;
  severity: ToastSeverity;
  summary: string;
  detail?: string;
  fieldErrors?: string[];
  durationMs?: number;
}

@Injectable({
  providedIn: 'root'
})
export class NotificationService {
  private readonly _messages = signal<ToastMessage[]>([]);
  public readonly messages = this._messages.asReadonly();

  show(message: Omit<ToastMessage, 'id'>): string {
    const id = Math.random().toString(36).substring(2, 9);
    const toast: ToastMessage = {
      id,
      durationMs: 5000,
      ...message
    };

    this._messages.update(prev => [...prev, toast]);

    if (toast.durationMs && toast.durationMs > 0) {
      setTimeout(() => {
        this.dismiss(id);
      }, toast.durationMs);
    }

    return id;
  }

  showSuccess(summary: string, detail?: string): string {
    return this.show({ severity: 'success', summary, detail });
  }

  showInfo(summary: string, detail?: string): string {
    return this.show({ severity: 'info', summary, detail });
  }

  showWarning(summary: string, detail?: string): string {
    return this.show({ severity: 'warn', summary, detail });
  }

  showError(summary: string, detail?: string): string {
    return this.show({ severity: 'error', summary, detail });
  }

  showProblemDetails(problem: ApiProblemDetails): string {
    const fieldErrors: string[] = [];
    if (problem.errors) {
      for (const [field, errors] of Object.entries(problem.errors)) {
        for (const err of errors) {
          fieldErrors.push(`${field}: ${err}`);
        }
      }
    }

    const summary = problem.title || 'Request Failed';
    const detail = problem.detail || (problem.code ? `Error code: ${problem.code}` : 'An unexpected error occurred.');

    return this.show({
      severity: 'error',
      summary,
      detail,
      fieldErrors: fieldErrors.length > 0 ? fieldErrors : undefined,
      durationMs: 7000
    });
  }

  dismiss(id: string): void {
    this._messages.update(prev => prev.filter(m => m.id !== id));
  }

  clear(): void {
    this._messages.set([]);
  }
}
