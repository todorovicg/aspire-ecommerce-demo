import { importProvidersFrom } from '@angular/core';
import { TranslocoTestingModule } from '@jsverse/transloco';
import { provideTranslocoLocale } from '@jsverse/transloco-locale';

import en from '../../../../public/i18n/en.json';
import sr from '../../../../public/i18n/sr.json';
import { defaultLanguage, languages, locales } from './languages';

// Specs render with the real dictionaries, so text assertions read English by default
export function provideTranslocoTesting() {
  return [
    importProvidersFrom(
      TranslocoTestingModule.forRoot({
        langs: { en, sr },
        translocoConfig: {
          availableLangs: [...languages],
          defaultLang: defaultLanguage,
          reRenderOnLangChange: true,
        },
        preloadLangs: true,
      }),
    ),
    provideTranslocoLocale({ langToLocaleMapping: locales }),
  ];
}
