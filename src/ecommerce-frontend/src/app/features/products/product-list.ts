import { ChangeDetectionStrategy, Component, computed, inject } from '@angular/core';
import { MatButtonModule } from '@angular/material/button';
import { MatCardModule } from '@angular/material/card';
import { MatProgressBarModule } from '@angular/material/progress-bar';
import { MatSnackBar } from '@angular/material/snack-bar';
import { TranslocoPipe, TranslocoService } from '@jsverse/transloco';
import { TranslocoCurrencyPipe } from '@jsverse/transloco-locale';

import { toApiFailure } from '../../core/api/api-response';
import { CatalogApi } from '../../core/api/catalog-api';
import { BasketStore } from '../../core/basket-store';
import { failureMessage } from '../../core/i18n/translate-known';
import { LanguageStore } from '../../core/language-store';
import { Product } from '../../shared/models/product';
import { ProductImage } from '../../shared/product-image/product-image';

@Component({
  selector: 'app-product-list',
  imports: [
    MatCardModule,
    MatButtonModule,
    MatProgressBarModule,
    ProductImage,
    TranslocoPipe,
    TranslocoCurrencyPipe,
  ],
  changeDetection: ChangeDetectionStrategy.OnPush,
  templateUrl: './product-list.html',
  styleUrl: './product-list.scss',
})
export class ProductList {
  private readonly catalog = inject(CatalogApi);
  private readonly snackBar = inject(MatSnackBar);
  private readonly transloco = inject(TranslocoService);
  private readonly language = inject(LanguageStore);

  protected readonly basket = inject(BasketStore);
  protected readonly products = this.catalog.listProducts();
  protected readonly failure = computed(() => {
    const error = this.products.error();
    return error ? toApiFailure(error) : null;
  });
  // Reads the language so the text follows a switch
  protected readonly failureText = computed(() => {
    this.language.language();
    const failure = this.failure();
    return failure ? failureMessage(this.transloco, failure) : null;
  });

  protected reload(): void {
    this.products.reload();
  }

  protected async add(product: Product): Promise<void> {
    try {
      const result = await this.basket.add(product.id);
      const message = result.capped
        ? this.transloco.translate('products.capped', {
            count: result.appliedQuantity,
            name: product.name,
          })
        : this.transloco.translate('products.added', { name: product.name });
      this.snackBar.open(message, undefined, { duration: 3000 });
    } catch (error) {
      this.snackBar.open(
        failureMessage(this.transloco, toApiFailure(error)),
        this.transloco.translate('common.dismiss'),
        { duration: 5000 },
      );
    }
  }
}
