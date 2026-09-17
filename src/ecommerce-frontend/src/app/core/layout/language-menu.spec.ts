import { provideHttpClient } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { TestBed } from '@angular/core/testing';

import { Language } from '../i18n/languages';
import { provideTranslocoTesting } from '../i18n/transloco-testing';
import { LanguageStore } from '../language-store';
import { LanguageMenu } from './language-menu';

interface MenuOwner {
  select(language: Language): Promise<void>;
}

describe('LanguageMenu', () => {
  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [LanguageMenu],
      providers: [provideHttpClient(), provideHttpClientTesting(), provideTranslocoTesting()],
    }).compileComponents();
  });

  afterEach(() => {
    TestBed.inject(HttpTestingController).verify();
    document.documentElement.lang = 'en';
  });

  it('shows the active language on the trigger', () => {
    const fixture = TestBed.createComponent(LanguageMenu);
    fixture.detectChanges();

    expect((fixture.nativeElement as HTMLElement).textContent).toContain('English');
  });

  it('selecting a language switches the store and stores the choice', async () => {
    const fixture = TestBed.createComponent(LanguageMenu);
    fixture.detectChanges();
    const controller = TestBed.inject(HttpTestingController);

    const selected = (fixture.componentInstance as unknown as MenuOwner).select('sr');
    await new Promise((resolve) => setTimeout(resolve));
    controller
      .expectOne((request) => request.method === 'PUT' && request.url === '/api/preferences')
      .flush({ success: true, data: { language: 'sr' }, error: null });
    await selected;
    fixture.detectChanges();

    expect(TestBed.inject(LanguageStore).language()).toBe('sr');
    expect((fixture.nativeElement as HTMLElement).textContent).toContain('Srpski');
  });
});
