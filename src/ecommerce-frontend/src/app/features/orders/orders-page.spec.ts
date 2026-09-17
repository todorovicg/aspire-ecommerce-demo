import { provideHttpClient } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { TestBed } from '@angular/core/testing';
import { provideRouter } from '@angular/router';

import { provideTranslocoTesting } from '../../core/i18n/transloco-testing';
import { PendingOrder } from '../../core/pending-order';
import { Order } from '../../shared/models/order';
import { OrdersPage } from './orders-page';

function order(id: string, status: Order['status']): Order {
  return {
    id,
    status,
    total: 10,
    currency: 'EUR',
    createdAt: '2026-09-12T12:00:00Z',
    paidAt: null,
    shippedAt: null,
    failureReason: null,
    cardLast4: '4242',
    buyer: { name: 'Goran', email: 'goran@example.com' },
    shippingAddress: {
      street: 'Main Street 1',
      city: 'Belgrade',
      postalCode: '11000',
      country: 'Serbia',
    },
    lines: [],
  };
}

describe('OrdersPage', () => {
  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [OrdersPage],
      providers: [
        provideRouter([]),
        provideHttpClient(),
        provideHttpClientTesting(),
        provideTranslocoTesting(),
      ],
    }).compileComponents();
  });

  async function render(orders: Order[]): Promise<HTMLElement> {
    const fixture = TestBed.createComponent(OrdersPage);
    fixture.detectChanges();
    const controller = TestBed.inject(HttpTestingController);
    controller.expectOne('/api/orders').flush({ success: true, data: orders, error: null });
    await fixture.whenStable();
    fixture.detectChanges();
    controller.verify();
    return fixture.nativeElement as HTMLElement;
  }

  it('shows the waiting notice instead of the empty state while the placed order is missing', async () => {
    TestBed.inject(PendingOrder).placed('o-1');

    const element = await render([]);

    expect(element.textContent).toContain('Your order is being placed');
    expect(element.textContent).not.toContain('You have not placed any orders yet');
    expect(element.textContent).toContain('Watching for status changes');
  });

  it('forgets the placed order once the list contains it', async () => {
    const pending = TestBed.inject(PendingOrder);
    pending.placed('o-1');

    const element = await render([order('o-1', 'Shipped')]);

    expect(pending.orderId()).toBeNull();
    expect(element.textContent).not.toContain('Your order is being placed');
    expect(element.textContent).not.toContain('Watching for status changes');
  });

  it('shows the empty state when nothing is pending', async () => {
    const element = await render([]);

    expect(element.textContent).toContain('You have not placed any orders yet');
    expect(element.textContent).not.toContain('Watching for status changes');
  });
});
