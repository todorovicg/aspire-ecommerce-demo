import { DOCUMENT, Injectable, computed, inject, signal } from '@angular/core';
import { TranslocoService } from '@jsverse/transloco';
import { firstValueFrom } from 'rxjs';

import { PreferencesApi } from './api/preferences-api';
import { Language, acceptLanguages, defaultLanguage, isLanguage } from './i18n/languages';

// The active language: read from the buyer's server-side preference before the first render, stored on every change
@Injectable({ providedIn: 'root' })
export class LanguageStore {
  private readonly transloco = inject(TranslocoService);
  private readonly preferences = inject(PreferencesApi);
  private readonly document = inject(DOCUMENT);
  private readonly languageState = signal<Language>(defaultLanguage);

  readonly language = this.languageState.asReadonly();
  readonly acceptLanguage = computed(() => acceptLanguages[this.languageState()]);

  async initialize(): Promise<void> {
    let language = defaultLanguage;
    try {
      const stored = await this.preferences.get();
      if (isLanguage(stored.language)) {
        language = stored.language;
      }
    } catch {
      // The shop opens even when the preference service is down; English is the default
    }
    await this.apply(language);
  }

  // The language switches at once; a failed store leaves it active for this visit and is reported by the caller
  async select(language: Language): Promise<void> {
    if (language === this.languageState()) {
      return;
    }
    await this.apply(language);
    await this.preferences.update(language);
  }

  private async apply(language: Language): Promise<void> {
    await firstValueFrom(this.transloco.load(language));
    this.transloco.setActiveLang(language);
    this.languageState.set(language);
    this.document.documentElement.lang = language;
  }
}
