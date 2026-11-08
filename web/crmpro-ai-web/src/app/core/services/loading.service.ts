import { Injectable, signal } from '@angular/core';

@Injectable({
  providedIn: 'root'
})
export class LoadingService {
  private activeRequests = 0;
  private readonly _isLoading = signal<boolean>(false);
  public readonly isLoading = this._isLoading.asReadonly();

  startLoading(): void {
    this.activeRequests++;
    if (this.activeRequests === 1) {
      this._isLoading.set(true);
    }
  }

  stopLoading(): void {
    if (this.activeRequests > 0) {
      this.activeRequests--;
    }
    if (this.activeRequests === 0) {
      this._isLoading.set(false);
    }
  }

  reset(): void {
    this.activeRequests = 0;
    this._isLoading.set(false);
  }
}
