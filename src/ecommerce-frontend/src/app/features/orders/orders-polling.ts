import { Order } from '../../shared/models/order';

export const ordersPollIntervalMs = 3000;

// Poll while an order can still change, or while the order placed a moment ago is not listed yet;
// Shipped and PaymentFailed are terminal
export function shouldPollOrders(
  orders: readonly Order[] | undefined,
  pendingOrderId: string | null = null,
): boolean {
  if (pendingOrderId !== null && !orders?.some((order) => order.id === pendingOrderId)) {
    return true;
  }
  return orders?.some((order) => order.status === 'Submitted' || order.status === 'Paid') ?? false;
}
