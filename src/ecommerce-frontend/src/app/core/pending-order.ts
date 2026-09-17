import { Injectable, signal } from '@angular/core';

// Remembers the order placed a moment ago until the orders page has seen it in the list
@Injectable({ providedIn: 'root' })
export class PendingOrder {
  private readonly orderIdState = signal<string | null>(null);

  readonly orderId = this.orderIdState.asReadonly();

  placed(orderId: string): void {
    this.orderIdState.set(orderId);
  }

  seen(orderId: string): void {
    if (this.orderIdState() === orderId) {
      this.orderIdState.set(null);
    }
  }
}
