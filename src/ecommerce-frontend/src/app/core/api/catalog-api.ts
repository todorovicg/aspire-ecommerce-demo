import { httpResource } from '@angular/common/http';
import { Injectable, Injector, inject } from '@angular/core';

import { Product } from '../../shared/models/product';
import { LanguageStore } from '../language-store';
import { ApiResponse, unwrap } from './api-response';

@Injectable({ providedIn: 'root' })
export class CatalogApi {
  private readonly injector = inject(Injector);
  private readonly language = inject(LanguageStore);

  // The header is part of the request, so the list reloads when the language changes
  listProducts() {
    return httpResource<Product[]>(
      () => ({
        url: '/api/catalog/products',
        headers: { 'Accept-Language': this.language.acceptLanguage() },
      }),
      {
        injector: this.injector,
        parse: (raw) => unwrap(raw as ApiResponse<Product[]>),
      },
    );
  }
}
