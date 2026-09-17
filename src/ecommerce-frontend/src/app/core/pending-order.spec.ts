import { TestBed } from '@angular/core/testing';

import { PendingOrder } from './pending-order';

describe('PendingOrder', () => {
  it('starts empty, remembers the placed order and forgets it once seen', () => {
    const pending = TestBed.inject(PendingOrder);
    expect(pending.orderId()).toBeNull();

    pending.placed('o-1');
    expect(pending.orderId()).toBe('o-1');

    pending.seen('o-2');
    expect(pending.orderId()).toBe('o-1');

    pending.seen('o-1');
    expect(pending.orderId()).toBeNull();
  });
});
