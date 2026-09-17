import { provideHttpClient } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { TestBed } from '@angular/core/testing';
import { TranslocoService } from '@jsverse/transloco';

import { provideTranslocoTesting } from './i18n/transloco-testing';
import { LanguageStore } from './language-store';

describe('LanguageStore', () => {
  let store: LanguageStore;
  let controller: HttpTestingController;

  beforeEach(() => {
    TestBed.configureTestingModule({
      providers: [provideHttpClient(), provideHttpClientTesting(), provideTranslocoTesting()],
    });
    store = TestBed.inject(LanguageStore);
    controller = TestBed.inject(HttpTestingController);
  });

  afterEach(() => {
    controller.verify();
    document.documentElement.lang = 'en';
  });

  it('starts in English', () => {
    expect(store.language()).toBe('en');
    expect(store.acceptLanguage()).toBe('en');
  });

  it('applies the stored preference before the first render', async () => {
    const initialized = store.initialize();
    controller
      .expectOne('/api/preferences')
      .flush({ success: true, data: { language: 'sr' }, error: null });
    await initialized;

    expect(store.language()).toBe('sr');
    expect(store.acceptLanguage()).toBe('sr-Latn');
    expect(TestBed.inject(TranslocoService).getActiveLang()).toBe('sr');
    expect(document.documentElement.lang).toBe('sr');
  });

  it('keeps English when the preference cannot be read', async () => {
    const initialized = store.initialize();
    controller
      .expectOne('/api/preferences')
      .flush(null, { status: 503, statusText: 'Service Unavailable' });
    await initialized;

    expect(store.language()).toBe('en');
    expect(document.documentElement.lang).toBe('en');
  });

  it('switches at once and stores the choice', async () => {
    const selected = store.select('sr');
    await new Promise((resolve) => setTimeout(resolve));

    expect(store.language()).toBe('sr');
    const request = controller.expectOne(
      (candidate) => candidate.method === 'PUT' && candidate.url === '/api/preferences',
    );
    expect(request.request.body).toEqual({ language: 'sr' });
    request.flush({ success: true, data: { language: 'sr' }, error: null });
    await selected;
  });

  it('does nothing when the language is already active', async () => {
    await store.select('en');

    expect(store.language()).toBe('en');
  });
});
