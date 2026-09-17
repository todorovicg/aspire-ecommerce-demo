import { provideHttpClient } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { TestBed } from '@angular/core/testing';

import { Basket } from '../shared/models/basket';
import { ApiFailure } from './api/api-response';
import { BasketStore } from './basket-store';

const buyerId = 'b0000000-0000-4000-8000-000000000001';
const mugId = 'a0000000-0000-4000-8000-000000000001';

function basketWith(quantity: number): Basket {
  const items =
    quantity === 0
      ? []
      : [
          {
            productId: mugId,
            productName: 'Aspire Ceramic Mug',
            unitPrice: 12.5,
            quantity,
            lineTotal: 12.5 * quantity,
          },
        ];
  return { buyerId, items, itemCount: quantity, total: 12.5 * quantity };
}

describe('BasketStore', () => {
  let store: BasketStore;
  let controller: HttpTestingController;

  beforeEach(() => {
    TestBed.configureTestingModule({
      providers: [provideHttpClient(), provideHttpClientTesting()],
    });
    store = TestBed.inject(BasketStore);
    controller = TestBed.inject(HttpTestingController);
  });

  afterEach(() => controller.verify());

  it('load fills the basket and the count', async () => {
    const loading = store.load();
    controller.expectOne('/api/basket').flush({ success: true, data: basketWith(2), error: null });
    await loading;

    expect(store.count()).toBe(2);
    expect(store.total()).toBe(25);
    expect(store.failure()).toBeNull();
  });

  it('load records a failure instead of throwing', async () => {
    const loading = store.load();
    controller.expectOne('/api/basket').flush(
      {
        success: false,
        data: null,
        error: {
          code: 'common.upstream_unavailable',
          message: 'An upstream service is unavailable',
          details: null,
          traceId: null,
        },
      },
      { status: 504, statusText: 'Gateway Timeout' },
    );
    await loading;

    expect(store.count()).toBe(0);
    expect(store.failure()?.code).toBe('common.upstream_unavailable');
  });

  it('setQuantity sends the line and replaces the basket with the result', async () => {
    const changing = store.setQuantity(mugId, 3);
    const request = controller.expectOne('/api/basket/items');
    expect(request.request.method).toBe('PUT');
    expect(request.request.body).toEqual({ productId: mugId, quantity: 3 });
    request.flush({
      success: true,
      data: { basket: basketWith(3), requestedQuantity: 3, appliedQuantity: 3, capped: false },
      error: null,
    });

    const result = await changing;

    expect(result.capped).toBe(false);
    expect(store.count()).toBe(3);
  });

  it('add requests one more than the current line quantity', async () => {
    const loading = store.load();
    controller.expectOne('/api/basket').flush({ success: true, data: basketWith(2), error: null });
    await loading;

    const adding = store.add(mugId);
    const request = controller.expectOne('/api/basket/items');
    expect(request.request.body).toEqual({ productId: mugId, quantity: 3 });
    request.flush({
      success: true,
      data: { basket: basketWith(3), requestedQuantity: 3, appliedQuantity: 3, capped: false },
      error: null,
    });
    await adding;

    expect(store.count()).toBe(3);
  });

  it('setQuantity throws an ApiFailure carrying the envelope code', async () => {
    const changing = store.setQuantity(mugId, 1);
    controller.expectOne('/api/basket/items').flush(
      {
        success: false,
        data: null,
        error: {
          code: 'basket.product_out_of_stock',
          message: 'Product [Mug] has [0] units available',
          details: null,
          traceId: null,
        },
      },
      { status: 409, statusText: 'Conflict' },
    );

    await expect(changing).rejects.toMatchObject({ code: 'basket.product_out_of_stock' });
    await expect(changing).rejects.toBeInstanceOf(ApiFailure);
    expect(store.busy()).toBe(false);
  });

  it('checkout posts the details and empties the local basket', async () => {
    const loading = store.load();
    controller.expectOne('/api/basket').flush({ success: true, data: basketWith(2), error: null });
    await loading;

    const request = {
      buyer: { name: 'Goran', email: 'goran@example.com' },
      shippingAddress: {
        street: 'Main Street 1',
        city: 'Belgrade',
        postalCode: '11000',
        country: 'Serbia',
      },
      card: { number: '4242424242424242', holderName: 'Goran' },
    };
    const checkingOut = store.checkout(request);
    const call = controller.expectOne('/api/basket/checkout');
    expect(call.request.method).toBe('POST');
    expect(call.request.body).toEqual(request);
    call.flush(
      { success: true, data: { orderId: 'c1', total: 25, currency: 'EUR' }, error: null },
      { status: 202, statusText: 'Accepted' },
    );
    const response = await checkingOut;

    expect(response.orderId).toBe('c1');
    expect(store.count()).toBe(0);
    expect(store.items()).toEqual([]);
  });

  it('remove sends a DELETE and replaces the basket', async () => {
    const removing = store.remove(mugId);
    const request = controller.expectOne(`/api/basket/items/${mugId}`);
    expect(request.request.method).toBe('DELETE');
    request.flush({ success: true, data: basketWith(0), error: null });
    await removing;

    expect(store.count()).toBe(0);
    expect(store.items()).toEqual([]);
  });
});
