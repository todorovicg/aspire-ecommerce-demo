import { HttpErrorResponse } from '@angular/common/http';

import { ApiFailure, toApiFailure, unwrap } from './api-response';

describe('unwrap', () => {
  it('returns data from a success envelope', () => {
    expect(unwrap({ success: true, data: [1, 2], error: null })).toEqual([1, 2]);
  });

  it('returns null data from an empty success envelope', () => {
    expect(unwrap<null>({ success: true, data: null, error: null })).toBeNull();
  });

  it('throws an ApiFailure carrying the envelope error', () => {
    const attempt = () =>
      unwrap({
        success: false,
        data: null,
        error: {
          code: 'basket.checkout_empty',
          message: 'Basket is empty',
          details: null,
          traceId: 't1',
        },
      });

    expect(attempt).toThrowError(ApiFailure);
    expect(attempt).toThrowError('Basket is empty');
  });
});

describe('toApiFailure', () => {
  it('maps an HTTP error whose body is an envelope', () => {
    const error = new HttpErrorResponse({
      status: 404,
      error: {
        success: false,
        data: null,
        error: {
          code: 'catalog.product_not_found',
          message: 'Product [x] not found',
          details: null,
          traceId: 't2',
        },
      },
    });

    const failure = toApiFailure(error);

    expect(failure.code).toBe('catalog.product_not_found');
    expect(failure.message).toBe('Product [x] not found');
    expect(failure.traceId).toBe('t2');
  });

  it('maps a network failure to client.network_error', () => {
    const failure = toApiFailure(
      new HttpErrorResponse({ status: 0, error: new ProgressEvent('error') }),
    );

    expect(failure.code).toBe('client.network_error');
  });

  it('maps an HTTP error without an envelope to its status code', () => {
    const failure = toApiFailure(
      new HttpErrorResponse({ status: 502, statusText: 'Bad Gateway', error: '' }),
    );

    expect(failure.code).toBe('common.http_502');
  });

  it('passes an ApiFailure through unchanged', () => {
    const original = new ApiFailure('x.y', 'msg');

    expect(toApiFailure(original)).toBe(original);
  });
});
