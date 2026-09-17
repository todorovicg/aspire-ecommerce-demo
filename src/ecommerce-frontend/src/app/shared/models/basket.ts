export interface BasketItem {
  productId: string;
  productName: string;
  unitPrice: number;
  quantity: number;
  lineTotal: number;
}

export interface Basket {
  buyerId: string;
  items: BasketItem[];
  itemCount: number;
  total: number;
}

export interface UpsertItemResult {
  basket: Basket;
  requestedQuantity: number;
  appliedQuantity: number;
  capped: boolean;
}
