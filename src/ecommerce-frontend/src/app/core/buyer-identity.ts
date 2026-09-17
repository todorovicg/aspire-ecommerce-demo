import { Injectable } from '@angular/core';

const storageKey = 'ecommerce-demo.buyer-id';

// One anonymous buyer id per browser, created on first use and kept in local storage
@Injectable({ providedIn: 'root' })
export class BuyerIdentity {
  readonly id: string = loadOrCreate();
}

function loadOrCreate(): string {
  try {
    const existing = localStorage.getItem(storageKey);
    if (existing) {
      return existing;
    }
    const created = crypto.randomUUID();
    localStorage.setItem(storageKey, created);
    return created;
  } catch {
    return crypto.randomUUID();
  }
}
