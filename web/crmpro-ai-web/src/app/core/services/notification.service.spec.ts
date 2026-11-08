import { describe, it, expect, beforeEach } from 'vitest';
import { NotificationService } from './notification.service';
import { ApiProblemDetails } from '../models/api-response.model';

describe('NotificationService', () => {
  let service: NotificationService;

  beforeEach(() => {
    service = new NotificationService();
  });

  it('should start with empty messages', () => {
    expect(service.messages().length).toBe(0);
  });

  it('should add a success message and allow dismissing it', () => {
    const id = service.showSuccess('Operation Succeeded', 'Lead saved successfully');
    expect(service.messages().length).toBe(1);
    expect(service.messages()[0].id).toBe(id);
    expect(service.messages()[0].severity).toBe('success');
    expect(service.messages()[0].summary).toBe('Operation Succeeded');

    service.dismiss(id);
    expect(service.messages().length).toBe(0);
  });

  it('should format RFC 7807 ApiProblemDetails correctly', () => {
    const problem: ApiProblemDetails = {
      type: 'https://crmpro.ai/errors/validation-failed',
      title: 'Validation Failed',
      status: 400,
      detail: 'Request validation failed.',
      code: 'VALIDATION_ERROR',
      errors: {
        Email: ['A valid email is required.'],
        EstimatedValue: ['Estimated value must be greater than zero.']
      }
    };

    const id = service.showProblemDetails(problem);
    expect(service.messages().length).toBe(1);

    const message = service.messages()[0];
    expect(message.id).toBe(id);
    expect(message.severity).toBe('error');
    expect(message.summary).toBe('Validation Failed');
    expect(message.fieldErrors).toBeDefined();
    expect(message.fieldErrors?.length).toBe(2);
    expect(message.fieldErrors).toContain('Email: A valid email is required.');
  });

  it('should clear all messages', () => {
    service.showInfo('Info 1');
    service.showWarning('Warn 1');
    expect(service.messages().length).toBe(2);

    service.clear();
    expect(service.messages().length).toBe(0);
  });
});
