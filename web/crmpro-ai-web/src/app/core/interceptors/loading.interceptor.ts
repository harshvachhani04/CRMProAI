import { HttpInterceptorFn } from '@angular/common/http';
import { inject } from '@angular/core';
import { finalize } from 'rxjs/operators';
import { LoadingService } from '../services/loading.service';

export const loadingInterceptor: HttpInterceptorFn = (req, next) => {
  // Allow silent background requests if header X-Silent is present
  if (req.headers.has('X-Silent')) {
    const stripped = req.clone({
      headers: req.headers.delete('X-Silent')
    });
    return next(stripped);
  }

  const loadingService = inject(LoadingService);
  loadingService.startLoading();

  return next(req).pipe(
    finalize(() => {
      loadingService.stopLoading();
    })
  );
};
