import { Routes } from '@angular/router';

export const routes: Routes = [
  { path: '', pathMatch: 'full', redirectTo: 'products' },
  {
    path: 'products',
    loadComponent: () => import('./features/products/product-list').then((m) => m.ProductList),
  },
  {
    path: 'basket',
    loadComponent: () => import('./features/basket/basket-page').then((m) => m.BasketPage),
  },
  {
    path: 'checkout',
    loadComponent: () => import('./features/checkout/checkout-page').then((m) => m.CheckoutPage),
  },
  {
    path: 'orders',
    loadComponent: () => import('./features/orders/orders-page').then((m) => m.OrdersPage),
  },
  { path: '**', redirectTo: 'products' },
];
