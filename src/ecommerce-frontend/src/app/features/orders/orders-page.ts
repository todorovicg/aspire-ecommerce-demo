import {
  ChangeDetectionStrategy,
  Component,
  DestroyRef,
  computed,
  effect,
  inject,
} from '@angular/core';
import { MatButtonModule } from '@angular/material/button';
import { MatCardModule } from '@angular/material/card';
import { MatProgressBarModule } from '@angular/material/progress-bar';
import { RouterLink } from '@angular/router';
import { TranslocoPipe, TranslocoService } from '@jsverse/transloco';
import { TranslocoCurrencyPipe, TranslocoDatePipe } from '@jsverse/transloco-locale';

import { toApiFailure } from '../../core/api/api-response';
import { OrdersApi } from '../../core/api/orders-api';
import { failureMessage, translateKnown } from '../../core/i18n/translate-known';
import { LanguageStore } from '../../core/language-store';
import { PendingOrder } from '../../core/pending-order';
import { ordersPollIntervalMs, shouldPollOrders } from './orders-polling';

@Component({
  selector: 'app-orders-page',
  imports: [
    MatCardModule,
    MatButtonModule,
    MatProgressBarModule,
    RouterLink,
    TranslocoPipe,
    TranslocoCurrencyPipe,
    TranslocoDatePipe,
  ],
  changeDetection: ChangeDetectionStrategy.OnPush,
  templateUrl: './orders-page.html',
  styleUrl: './orders-page.scss',
})
export class OrdersPage {
  private readonly api = inject(OrdersApi);
  private readonly pending = inject(PendingOrder);
  private readonly destroyRef = inject(DestroyRef);
  private readonly transloco = inject(TranslocoService);
  private readonly language = inject(LanguageStore);

  protected readonly orders = this.api.listOrders();
  protected readonly failure = computed(() => {
    const error = this.orders.error();
    return error ? toApiFailure(error) : null;
  });
  // Reads the language so the text follows a switch
  protected readonly failureText = computed(() => {
    this.language.language();
    const failure = this.failure();
    return failure ? failureMessage(this.transloco, failure) : null;
  });
  protected readonly polling = computed(() =>
    shouldPollOrders(this.orders.value(), this.pending.orderId()),
  );
  protected readonly waitingForOrder = computed(() => {
    const pendingId = this.pending.orderId();
    return pendingId !== null && !this.orders.value()?.some((order) => order.id === pendingId);
  });

  constructor() {
    // Refreshes only while an order is still moving or expected, so the dashboard is not flooded with idle requests
    const timer = setInterval(() => {
      if (this.polling() && this.orders.status() === 'resolved') {
        this.orders.reload();
      }
    }, ordersPollIntervalMs);
    this.destroyRef.onDestroy(() => clearInterval(timer));

    effect(() => {
      const pendingId = this.pending.orderId();
      if (pendingId !== null && this.orders.value()?.some((order) => order.id === pendingId)) {
        this.pending.seen(pendingId);
      }
    });
  }

  protected reload(): void {
    this.orders.reload();
  }

  // A known reason reads naturally in the active language; anything else is shown as the server sent it
  protected reason(failureReason: string): string {
    return translateKnown(this.transloco, `reasons.${failureReason}`, failureReason);
  }
}
