import { Order } from '../../shared/models/order';
import { shouldPollOrders } from './orders-polling';

function order(status: Order['status']): Order {
  return {
    id: `o-${status}`,
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

describe('shouldPollOrders', () => {
  it('is false without orders', () => {
    expect(shouldPollOrders(undefined)).toBe(false);
    expect(shouldPollOrders([])).toBe(false);
  });

  it('is false when every order is terminal', () => {
    expect(shouldPollOrders([order('Shipped'), order('PaymentFailed')])).toBe(false);
  });

  it('is true while an order is Submitted or Paid', () => {
    expect(shouldPollOrders([order('Shipped'), order('Submitted')])).toBe(true);
    expect(shouldPollOrders([order('Paid')])).toBe(true);
  });
});

describe('shouldPollOrders with a pending order', () => {
  it('is true while the order just placed is not listed yet', () => {
    expect(shouldPollOrders(undefined, 'o-new')).toBe(true);
    expect(shouldPollOrders([], 'o-new')).toBe(true);
    expect(shouldPollOrders([order('Shipped')], 'o-new')).toBe(true);
  });

  it('falls back to the status rule once the placed order is listed', () => {
    expect(shouldPollOrders([order('Shipped')], 'o-Shipped')).toBe(false);
    expect(shouldPollOrders([order('Paid')], 'o-Paid')).toBe(true);
  });
});
