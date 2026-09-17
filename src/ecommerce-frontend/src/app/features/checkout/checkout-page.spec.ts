import { provideHttpClient } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { TestBed } from '@angular/core/testing';
import { provideRouter } from '@angular/router';

import { provideTranslocoTesting } from '../../core/i18n/transloco-testing';
import { PendingOrder } from '../../core/pending-order';
import { CheckoutRequest } from '../../shared/models/order';
import { CheckoutPage } from './checkout-page';

interface FormOwner {
  form: CheckoutPage['form'];
  toRequest(): CheckoutRequest;
  submit(): Promise<void>;
}

describe('CheckoutPage', () => {
  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [CheckoutPage],
      providers: [
        provideRouter([]),
        provideHttpClient(),
        provideHttpClientTesting(),
        provideTranslocoTesting(),
      ],
    }).compileComponents();
  });

  function create(): FormOwner {
    return TestBed.createComponent(CheckoutPage).componentInstance as unknown as FormOwner;
  }

  it('starts with the approving test card and an otherwise invalid form', () => {
    const page = create();

    expect(page.form.controls.cardNumber.value).toBe('4242 4242 4242 4242');
    expect(page.form.valid).toBe(false);
  });

  it('becomes valid with complete details and strips spaces from the card number', () => {
    const page = create();
    page.form.setValue({
      name: 'Goran',
      email: 'goran@example.com',
      street: 'Main Street 1',
      city: 'Belgrade',
      postalCode: '11000',
      country: 'Serbia',
      cardNumber: '4242 4242 4242 4242',
      cardHolder: 'Goran',
    });

    expect(page.form.valid).toBe(true);
    expect(page.toRequest()).toEqual({
      buyer: { name: 'Goran', email: 'goran@example.com' },
      shippingAddress: {
        street: 'Main Street 1',
        city: 'Belgrade',
        postalCode: '11000',
        country: 'Serbia',
      },
      card: { number: '4242424242424242', holderName: 'Goran' },
    });
  });

  it('rejects a card number that is not 16 digits', () => {
    const page = create();
    page.form.controls.cardNumber.setValue('1234');

    expect(page.form.controls.cardNumber.valid).toBe(false);
  });
});

describe('CheckoutPage after placing an order', () => {
  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [CheckoutPage],
      providers: [
        provideRouter([{ path: 'orders', component: CheckoutPage }]),
        provideHttpClient(),
        provideHttpClientTesting(),
        provideTranslocoTesting(),
      ],
    }).compileComponents();
  });

  it('remembers the placed order id for the orders page', async () => {
    const page = TestBed.createComponent(CheckoutPage).componentInstance as unknown as FormOwner;
    page.form.setValue({
      name: 'Goran',
      email: 'goran@example.com',
      street: 'Main Street 1',
      city: 'Belgrade',
      postalCode: '11000',
      country: 'Serbia',
      cardNumber: '4242 4242 4242 4242',
      cardHolder: 'Goran',
    });
    const controller = TestBed.inject(HttpTestingController);

    const submitted = page.submit();
    controller.expectOne('/api/basket/checkout').flush({
      success: true,
      data: { orderId: 'o-1', total: 12.5, currency: 'EUR' },
      error: null,
    });
    await submitted;

    expect(TestBed.inject(PendingOrder).orderId()).toBe('o-1');
    controller.verify();
  });
});
