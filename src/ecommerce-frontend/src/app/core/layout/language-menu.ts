import { ChangeDetectionStrategy, Component, inject } from '@angular/core';
import { MatButtonModule } from '@angular/material/button';
import { MatMenuModule } from '@angular/material/menu';
import { MatSnackBar } from '@angular/material/snack-bar';
import { TranslocoPipe, TranslocoService } from '@jsverse/transloco';

import { Language, languageNames, languages } from '../i18n/languages';
import { LanguageStore } from '../language-store';

@Component({
  selector: 'app-language-menu',
  imports: [MatButtonModule, MatMenuModule, TranslocoPipe],
  changeDetection: ChangeDetectionStrategy.OnPush,
  templateUrl: './language-menu.html',
  styleUrl: './language-menu.scss',
})
export class LanguageMenu {
  private readonly snackBar = inject(MatSnackBar);
  private readonly transloco = inject(TranslocoService);

  protected readonly store = inject(LanguageStore);
  protected readonly languages = languages;
  protected readonly names = languageNames;

  protected async select(language: Language): Promise<void> {
    try {
      await this.store.select(language);
    } catch {
      this.snackBar.open(
        this.transloco.translate('language.saveFailed'),
        this.transloco.translate('common.dismiss'),
        { duration: 5000 },
      );
    }
  }
}
