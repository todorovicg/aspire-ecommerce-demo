import { ChangeDetectionStrategy, Component, inject } from '@angular/core';
import { MatBadgeModule } from '@angular/material/badge';
import { MatButtonModule } from '@angular/material/button';
import { MatToolbarModule } from '@angular/material/toolbar';
import { RouterLink, RouterLinkActive } from '@angular/router';
import { TranslocoPipe } from '@jsverse/transloco';

import { BasketStore } from '../basket-store';
import { LanguageMenu } from './language-menu';

@Component({
  selector: 'app-site-header',
  imports: [
    MatToolbarModule,
    MatButtonModule,
    MatBadgeModule,
    RouterLink,
    RouterLinkActive,
    TranslocoPipe,
    LanguageMenu,
  ],
  changeDetection: ChangeDetectionStrategy.OnPush,
  templateUrl: './site-header.html',
  styleUrl: './site-header.scss',
})
export class SiteHeader {
  protected readonly basket = inject(BasketStore);
}
