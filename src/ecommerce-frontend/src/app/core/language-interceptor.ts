import { HttpInterceptorFn } from '@angular/common/http';
import { inject } from '@angular/core';

import { LanguageStore } from './language-store';

// Every API call carries the active language unless the caller set the header itself
export const languageInterceptor: HttpInterceptorFn = (request, next) => {
  if (!request.url.startsWith('/api/') || request.headers.has('Accept-Language')) {
    return next(request);
  }
  const language = inject(LanguageStore);
  return next(request.clone({ setHeaders: { 'Accept-Language': language.acceptLanguage() } }));
};
