import { TestBed } from '@angular/core/testing';
import { TranslocoService } from '@jsverse/transloco';

import { ApiFailure } from '../api/api-response';
import { failureMessage, translateKnown } from './translate-known';
import { provideTranslocoTesting } from './transloco-testing';

describe('translateKnown', () => {
  let transloco: TranslocoService;

  beforeEach(() => {
    TestBed.configureTestingModule({ providers: [provideTranslocoTesting()] });
    transloco = TestBed.inject(TranslocoService);
  });

  it('translates a known key in the active language', () => {
    expect(translateKnown(transloco, 'reasons.card_declined', 'raw')).toBe('The card was declined');

    transloco.setActiveLang('sr');
    expect(translateKnown(transloco, 'reasons.card_declined', 'raw')).toBe('Kartica je odbijena');
  });

  it('returns the fallback for an unknown key', () => {
    expect(translateKnown(transloco, 'reasons.something_else', 'raw')).toBe('raw');
  });

  it('maps a known API error code and keeps the server message otherwise', () => {
    const known = new ApiFailure(
      'basket.product_out_of_stock',
      'Product [x] has [0] units available',
    );
    const unknown = new ApiFailure('basket.brand_new_code', 'Something specific');

    expect(failureMessage(transloco, known)).toBe('That product is out of stock');
    expect(failureMessage(transloco, unknown)).toBe('Something specific');
  });
});
