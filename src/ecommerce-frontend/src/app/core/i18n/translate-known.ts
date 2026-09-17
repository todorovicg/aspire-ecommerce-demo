import { TranslocoService } from '@jsverse/transloco';

import { ApiFailure } from '../api/api-response';

// Translates a key only when the active dictionary has it, otherwise returns the fallback text
export function translateKnown(transloco: TranslocoService, key: string, fallback: string): string {
  const table = transloco.getTranslation(transloco.getActiveLang());
  return key in table ? transloco.translate(key) : fallback;
}

// Known API error codes read naturally in the active language; anything else shows what the server said
export function failureMessage(transloco: TranslocoService, failure: ApiFailure): string {
  return translateKnown(transloco, `errors.${failure.code}`, failure.message);
}
