import { HttpInterceptorFn } from '@angular/common/http';

export const correlationIdInterceptor: HttpInterceptorFn = (req, next) => {
  if (req.headers.has('X-Correlation-ID')) {
    return next(req);
  }

  const correlationId = 'web-' + crypto.randomUUID();
  const modifiedReq = req.clone({
    headers: req.headers.set('X-Correlation-ID', correlationId)
  });

  return next(modifiedReq);
};
