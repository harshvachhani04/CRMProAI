import { HttpErrorResponse, HttpInterceptorFn } from '@angular/common/http';
import { inject } from '@angular/core';
import { catchError, throwError } from 'rxjs';
import { NotificationService } from '../services/notification.service';
import { ApiProblemDetails } from '../models/api-response.model';

export const errorInterceptor: HttpInterceptorFn = (req, next) => {
  const notificationService = inject(NotificationService);

  return next(req).pipe(
    catchError((error: unknown) => {
      if (error instanceof HttpErrorResponse) {
        // Skip toast if caller requested silent error handling
        if (!req.headers.has('X-Skip-Error-Toast')) {
          if (error.error && typeof error.error === 'object' && 'status' in error.error) {
            // RFC 7807 ProblemDetails from CRMPro AI backend
            const problem = error.error as ApiProblemDetails;
            notificationService.showProblemDetails(problem);
          } else if (error.status === 0) {
            notificationService.showError(
              'Network Error',
              'Unable to connect to the server. Please check your internet connection or try again.'
            );
          } else if (error.status === 401) {
            notificationService.showWarning(
              'Session Expired',
              'Your session has expired. Please log in again to continue.'
            );
          } else if (error.status === 403) {
            notificationService.showError(
              'Access Denied',
              'You do not have permission to perform this action.'
            );
          } else if (error.status === 404) {
            notificationService.showError(
              'Not Found',
              'The requested resource was not found.'
            );
          } else {
            notificationService.showError(
              'Server Error',
              error.statusText || 'An unexpected error occurred while communicating with the server.'
            );
          }
        }
      }

      return throwError(() => error);
    })
  );
};
