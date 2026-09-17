import { BuyerIdentity } from './buyer-identity';

describe('BuyerIdentity', () => {
  beforeEach(() => localStorage.clear());

  it('creates a UUID on first use and stores it', () => {
    const identity = new BuyerIdentity();

    expect(identity.id).toMatch(/^[0-9a-f]{8}-[0-9a-f]{4}-[0-9a-f]{4}-[0-9a-f]{4}-[0-9a-f]{12}$/);
    expect(localStorage.getItem('ecommerce-demo.buyer-id')).toBe(identity.id);
  });

  it('reuses the stored id', () => {
    localStorage.setItem('ecommerce-demo.buyer-id', 'stored-id');

    expect(new BuyerIdentity().id).toBe('stored-id');
  });
});
