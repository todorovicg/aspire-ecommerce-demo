import { provideHttpClient } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { TestBed } from '@angular/core/testing';
import { provideRouter } from '@angular/router';

import { App } from './app';
import { provideTranslocoTesting } from './core/i18n/transloco-testing';

describe('App', () => {
  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [App],
      providers: [
        provideRouter([]),
        provideHttpClient(),
        provideHttpClientTesting(),
        provideTranslocoTesting(),
      ],
    }).compileComponents();
  });

  it('renders the header, the language menu and the footer credit and loads the basket', async () => {
    const fixture = TestBed.createComponent(App);
    fixture.detectChanges();
    const controller = TestBed.inject(HttpTestingController);
    controller.expectOne('/api/basket').flush({
      success: true,
      data: { buyerId: 'b', items: [], itemCount: 0, total: 0 },
      error: null,
    });
    await fixture.whenStable();

    const text = (fixture.nativeElement as HTMLElement).textContent ?? '';
    expect(text).toContain('E-Commerce Aspire Demo');
    expect(text).toContain('English');
    expect(text).toContain('Products');
    expect(text).toContain('Basket');
    expect(text).toContain('Orders');
    expect(text).toContain('Goran Todorovic 2026 - Master');
    controller.verify();
  });
});
