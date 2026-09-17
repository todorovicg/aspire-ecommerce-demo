import { ChangeDetectionStrategy, Component, effect, inject } from '@angular/core';
import { RouterOutlet } from '@angular/router';

import { BasketStore } from './core/basket-store';
import { LanguageStore } from './core/language-store';
import { SiteFooter } from './core/layout/site-footer';
import { SiteHeader } from './core/layout/site-header';

@Component({
  selector: 'app-root',
  imports: [RouterOutlet, SiteHeader, SiteFooter],
  changeDetection: ChangeDetectionStrategy.OnPush,
  templateUrl: './app.html',
  styleUrl: './app.scss',
})
export class App {
  private readonly basket = inject(BasketStore);
  private readonly language = inject(LanguageStore);

  constructor() {
    // Loaded at startup and again on every language change, because line names come from Catalog in that language
    effect(() => {
      this.language.language();
      void this.basket.load();
    });
  }
}
