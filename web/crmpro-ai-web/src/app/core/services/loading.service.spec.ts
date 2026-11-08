import { describe, it, expect, beforeEach } from 'vitest';
import { LoadingService } from './loading.service';

describe('LoadingService', () => {
  let service: LoadingService;

  beforeEach(() => {
    service = new LoadingService();
  });

  it('should initialize with isLoading false', () => {
    expect(service.isLoading()).toBe(false);
  });

  it('should toggle isLoading when startLoading and stopLoading are called', () => {
    service.startLoading();
    expect(service.isLoading()).toBe(true);

    service.startLoading(); // second request
    expect(service.isLoading()).toBe(true);

    service.stopLoading(); // one request completed, one remaining
    expect(service.isLoading()).toBe(true);

    service.stopLoading(); // all requests completed
    expect(service.isLoading()).toBe(false);
  });

  it('should reset state cleanly', () => {
    service.startLoading();
    service.startLoading();
    expect(service.isLoading()).toBe(true);

    service.reset();
    expect(service.isLoading()).toBe(false);
  });
});
