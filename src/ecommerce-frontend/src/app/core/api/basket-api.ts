import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable, firstValueFrom } from 'rxjs';

import { Basket, UpsertItemResult } from '../../shared/models/basket';
import { CheckoutRequest, CheckoutResponse } from '../../shared/models/order';
import { ApiResponse, toApiFailure, unwrap } from './api-response';

@Injectable({ providedIn: 'root' })
export class BasketApi {
  private readonly http = inject(HttpClient);

  get(): Promise<Basket> {
    return this.request(this.http.get<ApiResponse<Basket>>('/api/basket'));
  }

  upsertItem(productId: string, quantity: number): Promise<UpsertItemResult> {
    return this.request(
      this.http.put<ApiResponse<UpsertItemResult>>('/api/basket/items', { productId, quantity }),
    );
  }

  removeItem(productId: string): Promise<Basket> {
    return this.request(this.http.delete<ApiResponse<Basket>>(`/api/basket/items/${productId}`));
  }

  checkout(request: CheckoutRequest): Promise<CheckoutResponse> {
    return this.request(
      this.http.post<ApiResponse<CheckoutResponse>>('/api/basket/checkout', request),
    );
  }

  private async request<T>(call: Observable<ApiResponse<T>>): Promise<T> {
    try {
      return unwrap(await firstValueFrom(call));
    } catch (error) {
      throw toApiFailure(error);
    }
  }
}
