import { provideHttpClient, withInterceptors } from '@angular/common/http';
import {
  ApplicationConfig,
  inject,
  isDevMode,
  provideAppInitializer,
  provideBrowserGlobalErrorListeners,
} from '@angular/core';
import { provideRouter } from '@angular/router';
import { provideTransloco } from '@jsverse/transloco';
import { provideTranslocoLocale } from '@jsverse/transloco-locale';

import { routes } from './app.routes';
import { buyerIdInterceptor } from './core/buyer-id-interceptor';
import { defaultLanguage, languages, locales } from './core/i18n/languages';
import { TranslocoHttpLoader } from './core/i18n/transloco-loader';
import { languageInterceptor } from './core/language-interceptor';
import { LanguageStore } from './core/language-store';

export const appConfig: ApplicationConfig = {
  providers: [
    provideBrowserGlobalErrorListeners(),
    provideRouter(routes),
    provideHttpClient(withInterceptors([buyerIdInterceptor, languageInterceptor])),
    provideTransloco({
      config: {
        availableLangs: [...languages],
        defaultLang: defaultLanguage,
        fallbackLang: defaultLanguage,
        reRenderOnLangChange: true,
        prodMode: !isDevMode(),
      },
      loader: TranslocoHttpLoader,
    }),
    provideTranslocoLocale({ langToLocaleMapping: locales }),
    // The buyer's language is known before the first render, so nothing flashes in English
    provideAppInitializer(() => inject(LanguageStore).initialize()),
  ],
};
