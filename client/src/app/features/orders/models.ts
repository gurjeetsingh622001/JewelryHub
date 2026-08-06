// Mirrors JewelryHub.Domain.Enums — numeric on the wire, see
// features/products/models.ts for why (no JsonStringEnumConverter).
export enum OrderStatus {
  PendingPayment = 0,
  Confirmed = 1,
  Processing = 2,
  Shipped = 3,
  Delivered = 4,
  Cancelled = 5,
  ReturnRequested = 6,
  Returned = 7,
  Refunded = 8,
}

export enum PaymentMethod {
  CreditCard = 0,
  DebitCard = 1,
  Upi = 2,
  NetBanking = 3,
  Wallet = 4,
  CashOnDelivery = 5,
}

export enum PaymentStatus {
  Pending = 0,
  Authorized = 1,
  Completed = 2,
  Failed = 3,
  Refunded = 4,
  PartiallyRefunded = 5,
}

export enum ShipmentStatus {
  Pending = 0,
  Packed = 1,
  Shipped = 2,
  OutForDelivery = 3,
  Delivered = 4,
  Failed = 5,
  ReturnedToSeller = 6,
}

export const ORDER_STATUS_LABELS: Record<OrderStatus, string> = {
  [OrderStatus.PendingPayment]: 'Pending Payment',
  [OrderStatus.Confirmed]: 'Confirmed',
  [OrderStatus.Processing]: 'Processing',
  [OrderStatus.Shipped]: 'Shipped',
  [OrderStatus.Delivered]: 'Delivered',
  [OrderStatus.Cancelled]: 'Cancelled',
  [OrderStatus.ReturnRequested]: 'Return Requested',
  [OrderStatus.Returned]: 'Returned',
  [OrderStatus.Refunded]: 'Refunded',
};

export const PAYMENT_METHOD_LABELS: Record<PaymentMethod, string> = {
  [PaymentMethod.CreditCard]: 'Credit Card',
  [PaymentMethod.DebitCard]: 'Debit Card',
  [PaymentMethod.Upi]: 'UPI',
  [PaymentMethod.NetBanking]: 'Net Banking',
  [PaymentMethod.Wallet]: 'Wallet',
  [PaymentMethod.CashOnDelivery]: 'Cash on Delivery',
};

// Mirrors OrderDto and friends.
export interface OrderTax {
  componentType: number;
  ratePercentageApplied: number;
  taxableAmount: number;
  taxAmount: number;
}

export interface Shipment {
  status: ShipmentStatus;
  carrier: string | null;
  trackingNumber: string | null;
  trackingUrl: string | null;
  shippedAtUtc: string | null;
  deliveredAtUtc: string | null;
}

export interface Payment {
  id: string;
  method: PaymentMethod;
  status: PaymentStatus;
  amount: number;
  paidAtUtc: string | null;
}

export interface OrderItem {
  id: string;
  productId: string;
  productNameSnapshot: string;
  sellerId: string;
  sellerBusinessName: string;
  unitPriceSnapshot: number;
  quantity: number;
  lineTotal: number;
  itemTaxTotal: number;
  itemStatus: OrderStatus;
  taxes: OrderTax[];
  shipment: Shipment | null;
}

export interface Order {
  id: string;
  orderNumber: string;
  status: OrderStatus;
  subtotal: number;
  totalTax: number;
  shippingCharges: number;
  discountAmount: number;
  grandTotal: number;
  shippingAddressSummary: string;
  customerNote: string | null;
  confirmedAtUtc: string | null;
  deliveredAtUtc: string | null;
  createdAtUtc: string;
  items: OrderItem[];
  payments: Payment[];
}

export interface OrderSummary {
  id: string;
  orderNumber: string;
  status: OrderStatus;
  grandTotal: number;
  itemCount: number;
  createdAtUtc: string;
}

// Mirrors CreateOrderCommand.
export interface CheckoutRequest {
  shippingAddressId: string;
  billingAddressId: string;
  paymentMethod: PaymentMethod;
  customerNote?: string | null;
}
