import { ChangeDetectionStrategy, Component, computed, inject } from '@angular/core';
import { MatButtonModule } from '@angular/material/button';
import { MatCardModule } from '@angular/material/card';
import { MatSnackBar } from '@angular/material/snack-bar';
import { RouterLink } from '@angular/router';
import { TranslocoPipe, TranslocoService } from '@jsverse/transloco';
import { TranslocoCurrencyPipe } from '@jsverse/transloco-locale';

import { toApiFailure } from '../../core/api/api-response';
import { BasketStore } from '../../core/basket-store';
import { failureMessage } from '../../core/i18n/translate-known';
import { LanguageStore } from '../../core/language-store';
import { BasketItem } from '../../shared/models/basket';

@Component({
  selector: 'app-basket-page',
  imports: [MatCardModule, MatButtonModule, RouterLink, TranslocoPipe, TranslocoCurrencyPipe],
  changeDetection: ChangeDetectionStrategy.OnPush,
  templateUrl: './basket-page.html',
  styleUrl: './basket-page.scss',
})
export class BasketPage {
  private readonly snackBar = inject(MatSnackBar);
  private readonly transloco = inject(TranslocoService);
  private readonly language = inject(LanguageStore);

  protected readonly store = inject(BasketStore);
  // Reads the language so the text follows a switch
  protected readonly failureText = computed(() => {
    this.language.language();
    const failure = this.store.failure();
    return failure ? failureMessage(this.transloco, failure) : null;
  });

  protected increase(item: BasketItem): Promise<void> {
    return this.change(item, item.quantity + 1);
  }

  protected decrease(item: BasketItem): Promise<void> {
    return item.quantity === 1 ? this.remove(item) : this.change(item, item.quantity - 1);
  }

  protected async remove(item: BasketItem): Promise<void> {
    try {
      await this.store.remove(item.productId);
      this.snackBar.open(
        this.transloco.translate('basket.removed', { name: item.productName }),
        undefined,
        { duration: 3000 },
      );
    } catch (error) {
      this.notify(error);
    }
  }

  private async change(item: BasketItem, quantity: number): Promise<void> {
    try {
      const result = await this.store.setQuantity(item.productId, quantity);
      if (result.capped) {
        this.snackBar.open(
          this.transloco.translate('basket.capped', {
            count: result.appliedQuantity,
            name: item.productName,
          }),
          undefined,
          { duration: 4000 },
        );
      }
    } catch (error) {
      this.notify(error);
    }
  }

  private notify(error: unknown): void {
    this.snackBar.open(
      failureMessage(this.transloco, toApiFailure(error)),
      this.transloco.translate('common.dismiss'),
      { duration: 5000 },
    );
  }
}
