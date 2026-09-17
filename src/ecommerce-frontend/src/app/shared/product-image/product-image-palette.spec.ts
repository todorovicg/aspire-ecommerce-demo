import { hueFor, initialsFor } from './product-image-palette';

describe('hueFor', () => {
  it('is deterministic and within the hue range', () => {
    expect(hueFor('mug')).toBe(hueFor('mug'));
    expect(hueFor('mug')).toBeGreaterThanOrEqual(0);
    expect(hueFor('mug')).toBeLessThan(360);
  });

  it('differs for different keys', () => {
    expect(hueFor('mug')).not.toBe(hueFor('hoodie'));
  });
});

describe('initialsFor', () => {
  it('takes the first letter of the first two words', () => {
    expect(initialsFor('Aspire Ceramic Mug')).toBe('AC');
  });

  it('handles a single word', () => {
    expect(initialsFor('Poster')).toBe('P');
  });
});
