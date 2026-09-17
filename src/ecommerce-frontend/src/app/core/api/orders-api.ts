import { httpResource } from '@angular/common/http';
import { Injectable, Injector, inject } from '@angular/core';

import { Order } from '../../shared/models/order';
import { ApiResponse, unwrap } from './api-response';

@Injectable({ providedIn: 'root' })
export class OrdersApi {
  private readonly injector = inject(Injector);

  listOrders() {
    return httpResource<Order[]>(() => '/api/orders', {
      injector: this.injector,
      parse: (raw) => unwrap(raw as ApiResponse<Order[]>),
    });
  }
}
