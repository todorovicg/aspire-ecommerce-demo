import { HttpClient, provideHttpClient, withInterceptors } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { TestBed } from '@angular/core/testing';

import { provideTranslocoTesting } from './i18n/transloco-testing';
import { languageInterceptor } from './language-interceptor';

describe('languageInterceptor', () => {
  let http: HttpClient;
  let controller: HttpTestingController;

  beforeEach(() => {
    TestBed.configureTestingModule({
      providers: [
        provideHttpClient(withInterceptors([languageInterceptor])),
        provideHttpClientTesting(),
        provideTranslocoTesting(),
      ],
    });
    http = TestBed.inject(HttpClient);
    controller = TestBed.inject(HttpTestingController);
  });

  afterEach(() => controller.verify());

  it('adds the active language to API requests', () => {
    http.get('/api/basket').subscribe();

    const request = controller.expectOne('/api/basket');
    expect(request.request.headers.get('Accept-Language')).toBe('en');
    request.flush({ success: true, data: null, error: null });
  });

  it('keeps a header the caller set', () => {
    http.get('/api/catalog/products', { headers: { 'Accept-Language': 'sr-Latn' } }).subscribe();

    const request = controller.expectOne('/api/catalog/products');
    expect(request.request.headers.get('Accept-Language')).toBe('sr-Latn');
    request.flush({ success: true, data: [], error: null });
  });

  it('leaves non-API requests untouched', () => {
    http.get('/i18n/en.json').subscribe();

    const request = controller.expectOne('/i18n/en.json');
    expect(request.request.headers.has('Accept-Language')).toBe(false);
    request.flush({});
  });
});
