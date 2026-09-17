export const languages = ['en', 'sr'] as const;

export type Language = (typeof languages)[number];

export const defaultLanguage: Language = 'en';

// Shown in the language menu, each name in its own language
export const languageNames: Record<Language, string> = { en: 'English', sr: 'Srpski' };

// What the API receives in Accept-Language; Serbian is served in the Latin script
export const acceptLanguages: Record<Language, string> = { en: 'en', sr: 'sr-Latn' };

// Locales for number and date formatting
export const locales: Record<Language, string> = { en: 'en-US', sr: 'sr-Latn-RS' };

export function isLanguage(value: unknown): value is Language {
  return typeof value === 'string' && (languages as readonly string[]).includes(value);
}
