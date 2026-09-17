import { ChangeDetectionStrategy, Component, inject, signal } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { MatButtonModule } from '@angular/material/button';
import { MatCardModule } from '@angular/material/card';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatSnackBar } from '@angular/material/snack-bar';
import { Router, RouterLink } from '@angular/router';
import { TranslocoPipe, TranslocoService } from '@jsverse/transloco';
import { TranslocoCurrencyPipe, TranslocoLocaleService } from '@jsverse/transloco-locale';

import { toApiFailure } from '../../core/api/api-response';
import { BasketStore } from '../../core/basket-store';
import { failureMessage } from '../../core/i18n/translate-known';
import { PendingOrder } from '../../core/pending-order';
import { CheckoutRequest } from '../../shared/models/order';

export const approvingTestCard = '4242 4242 4242 4242';

@Component({
  selector: 'app-checkout-page',
  imports: [
    ReactiveFormsModule,
    MatCardModule,
    MatButtonModule,
    MatFormFieldModule,
    MatInputModule,
    RouterLink,
    TranslocoPipe,
    TranslocoCurrencyPipe,
  ],
  changeDetection: ChangeDetectionStrategy.OnPush,
  templateUrl: './checkout-page.html',
  styleUrl: './checkout-page.scss',
})
export class CheckoutPage {
  private readonly formBuilder = inject(FormBuilder).nonNullable;
  private readonly router = inject(Router);
  private readonly snackBar = inject(MatSnackBar);
  private readonly pending = inject(PendingOrder);
  private readonly transloco = inject(TranslocoService);
  private readonly locale = inject(TranslocoLocaleService);

  protected readonly store = inject(BasketStore);
  protected readonly submitting = signal(false);

  protected readonly form = this.formBuilder.group({
    name: ['', [Validators.required, Validators.minLength(2), Validators.maxLength(100)]],
    email: ['', [Validators.required, Validators.email, Validators.maxLength(200)]],
    street: ['', [Validators.required, Validators.minLength(2), Validators.maxLength(200)]],
    city: ['', [Validators.required, Validators.minLength(2), Validators.maxLength(100)]],
    postalCode: ['', [Validators.required, Validators.minLength(2), Validators.maxLength(20)]],
    country: ['', [Validators.required, Validators.minLength(2), Validators.maxLength(100)]],
    cardNumber: [approvingTestCard, [Validators.required, Validators.pattern(/^(\d\s*){16}$/)]],
    cardHolder: ['', [Validators.required, Validators.minLength(2), Validators.maxLength(100)]],
  });

  protected toRequest(): CheckoutRequest {
    const value = this.form.getRawValue();
    return {
      buyer: { name: value.name.trim(), email: value.email.trim() },
      shippingAddress: {
        street: value.street.trim(),
        city: value.city.trim(),
        postalCode: value.postalCode.trim(),
        country: value.country.trim(),
      },
      card: { number: value.cardNumber.replace(/\s+/g, ''), holderName: value.cardHolder.trim() },
    };
  }

  protected async submit(): Promise<void> {
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }
    this.submitting.set(true);
    try {
      const response = await this.store.checkout(this.toRequest());
      this.pending.placed(response.orderId);
      const total = this.locale.localizeNumber(response.total, 'currency', undefined, {
        currency: response.currency,
      });
      this.snackBar.open(this.transloco.translate('checkout.placed', { total }), undefined, {
        duration: 4000,
      });
      await this.router.navigate(['/orders']);
    } catch (error) {
      this.snackBar.open(
        failureMessage(this.transloco, toApiFailure(error)),
        this.transloco.translate('common.dismiss'),
        { duration: 6000 },
      );
    } finally {
      this.submitting.set(false);
    }
  }
}
