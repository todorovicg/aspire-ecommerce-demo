export interface CheckoutRequest {
  buyer: { name: string; email: string };
  shippingAddress: { street: string; city: string; postalCode: string; country: string };
  card: { number: string; holderName: string };
}

export interface CheckoutResponse {
  orderId: string;
  total: number;
  currency: string;
}

export type OrderStatus = 'Submitted' | 'Paid' | 'Shipped' | 'PaymentFailed';

export interface OrderLine {
  productId: string;
  productName: string;
  unitPrice: number;
  quantity: number;
  lineTotal: number;
}

export interface Order {
  id: string;
  status: OrderStatus;
  total: number;
  currency: string;
  createdAt: string;
  paidAt: string | null;
  shippedAt: string | null;
  failureReason: string | null;
  cardLast4: string;
  buyer: { name: string; email: string };
  shippingAddress: { street: string; city: string; postalCode: string; country: string };
  lines: OrderLine[];
}
