import { HttpClient, provideHttpClient, withInterceptors } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { TestBed } from '@angular/core/testing';

import { buyerIdInterceptor } from './buyer-id-interceptor';
import { BuyerIdentity } from './buyer-identity';

describe('buyerIdInterceptor', () => {
  let http: HttpClient;
  let controller: HttpTestingController;

  beforeEach(() => {
    localStorage.clear();
    TestBed.configureTestingModule({
      providers: [
        provideHttpClient(withInterceptors([buyerIdInterceptor])),
        provideHttpClientTesting(),
      ],
    });
    http = TestBed.inject(HttpClient);
    controller = TestBed.inject(HttpTestingController);
  });

  afterEach(() => controller.verify());

  it('adds the X-Buyer-Id header to API requests', () => {
    http.get('/api/catalog/products').subscribe();

    const request = controller.expectOne('/api/catalog/products');
    expect(request.request.headers.get('X-Buyer-Id')).toBe(TestBed.inject(BuyerIdentity).id);
    request.flush({ success: true, data: [], error: null });
  });

  it('leaves non-API requests untouched', () => {
    http.get('/assets/config.json').subscribe();

    const request = controller.expectOne('/assets/config.json');
    expect(request.request.headers.has('X-Buyer-Id')).toBe(false);
    request.flush({});
  });
});
