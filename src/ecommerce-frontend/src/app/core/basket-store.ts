import { Injectable, computed, inject, signal } from '@angular/core';

import { Basket, UpsertItemResult } from '../shared/models/basket';
import { CheckoutRequest, CheckoutResponse } from '../shared/models/order';
import { ApiFailure, toApiFailure } from './api/api-response';
import { BasketApi } from './api/basket-api';

// The single source of truth for the basket in the browser; every mutation replaces it with the server's copy
@Injectable({ providedIn: 'root' })
export class BasketStore {
  private readonly api = inject(BasketApi);
  private readonly basketState = signal<Basket | null>(null);
  private readonly busyState = signal(false);
  private readonly failureState = signal<ApiFailure | null>(null);

  readonly basket = this.basketState.asReadonly();
  readonly busy = this.busyState.asReadonly();
  readonly failure = this.failureState.asReadonly();
  readonly items = computed(() => this.basketState()?.items ?? []);
  readonly count = computed(() => this.basketState()?.itemCount ?? 0);
  readonly total = computed(() => this.basketState()?.total ?? 0);

  async load(): Promise<void> {
    this.busyState.set(true);
    try {
      this.basketState.set(await this.api.get());
      this.failureState.set(null);
    } catch (error) {
      this.failureState.set(toApiFailure(error));
    } finally {
      this.busyState.set(false);
    }
  }

  add(productId: string): Promise<UpsertItemResult> {
    const existing = this.items().find((item) => item.productId === productId);
    return this.setQuantity(productId, (existing?.quantity ?? 0) + 1);
  }

  async setQuantity(productId: string, quantity: number): Promise<UpsertItemResult> {
    this.busyState.set(true);
    try {
      const result = await this.api.upsertItem(productId, quantity);
      this.basketState.set(result.basket);
      return result;
    } finally {
      this.busyState.set(false);
    }
  }

  async remove(productId: string): Promise<void> {
    this.busyState.set(true);
    try {
      this.basketState.set(await this.api.removeItem(productId));
    } finally {
      this.busyState.set(false);
    }
  }

  // On success the server has already cleared the basket, so the local copy is emptied too
  async checkout(request: CheckoutRequest): Promise<CheckoutResponse> {
    this.busyState.set(true);
    try {
      const response = await this.api.checkout(request);
      const current = this.basketState();
      this.basketState.set(current ? { ...current, items: [], itemCount: 0, total: 0 } : null);
      return response;
    } finally {
      this.busyState.set(false);
    }
  }
}
