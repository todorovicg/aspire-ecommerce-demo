import { HttpInterceptorFn } from '@angular/common/http';
import { inject } from '@angular/core';

import { BuyerIdentity } from './buyer-identity';

export const buyerIdInterceptor: HttpInterceptorFn = (request, next) => {
  if (!request.url.startsWith('/api/')) {
    return next(request);
  }
  const buyer = inject(BuyerIdentity);
  return next(request.clone({ setHeaders: { 'X-Buyer-Id': buyer.id } }));
};
