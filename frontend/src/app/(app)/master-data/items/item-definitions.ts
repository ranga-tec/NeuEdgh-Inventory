export type BrandDto = { id: string; code: string; name: string; isActive: boolean };

export type UomDto = { id: string; code: string; name: string; isActive: boolean };

export type CategoryDto = {
  id: string;
  code: string;
  name: string;
  revenueAccountId?: string | null;
  revenueAccountCode?: string | null;
  revenueAccountName?: string | null;
  expenseAccountId?: string | null;
  expenseAccountCode?: string | null;
  expenseAccountName?: string | null;
  isActive: boolean;
};

export type SubcategoryDto = {
  id: string;
  categoryId: string;
  categoryCode?: string | null;
  categoryName?: string | null;
  code: string;
  name: string;
  isActive: boolean;
};

export type ItemDto = {
  id: string;
  sku: string;
  name: string;
  type: number;
  trackingType: number;
  unitOfMeasure: string;
  brandId?: string | null;
  categoryId?: string | null;
  categoryCode?: string | null;
  categoryName?: string | null;
  subcategoryId?: string | null;
  subcategoryCode?: string | null;
  subcategoryName?: string | null;
  barcode?: string | null;
  defaultUnitCost: number;
  revenueAccountId?: string | null;
  revenueAccountCode?: string | null;
  revenueAccountName?: string | null;
  expenseAccountId?: string | null;
  expenseAccountCode?: string | null;
  expenseAccountName?: string | null;
  isActive: boolean;
};

export type LedgerAccountOptionDto = {
  id: string;
  code: string;
  name: string;
  accountType: number;
  allowsPosting: boolean;
  isActive: boolean;
};

export type ItemRef = { id: string; sku: string; name: string };

export type ItemAttachmentDto = {
  id: string;
  itemId: string;
  fileName: string;
  url: string;
  isImage: boolean;
  contentType?: string | null;
  sizeBytes?: number | null;
  notes?: string | null;
  createdAt: string;
  createdBy?: string | null;
};

export type ItemPriceHistoryDto = {
  auditLogId: string;
  occurredAt: string;
  userId?: string | null;
  oldDefaultUnitCost?: number | null;
  newDefaultUnitCost: number;
};

export const itemTypes = [
  { value: 1, label: "Stock Item" },
  { value: 2, label: "Consumable" },
  { value: 3, label: "Service / Utility" },
];

export const trackingTypes = [
  { value: 0, label: "None - no expiry or batch" },
  { value: 1, label: "Serial - unique units" },
  { value: 2, label: "Batch - batch only" },
  { value: 3, label: "Expiry - FEFO by expiry date" },
  { value: 4, label: "Batch + Expiry - batch with FEFO" },
];

export const itemTypeLabel: Record<number, string> = {
  1: "Stock Item",
  2: "Consumable",
  3: "Service / Utility",
};

export const trackingLabel: Record<number, string> = {
  0: "None",
  1: "Serial",
  2: "Batch",
  3: "Expiry",
  4: "Batch + Expiry",
};

export const issueMethodLabel: Record<number, string> = {
  0: "FIFO",
  1: "Serial",
  2: "FIFO by batch",
  3: "FEFO",
  4: "FEFO by batch",
};

export function isExpiryTracked(trackingType: number): boolean {
  return trackingType === 3 || trackingType === 4;
}

export function formatLedgerAccountOptionLabel(account: LedgerAccountOptionDto): string {
  const flags: string[] = [];

  if (!account.allowsPosting) {
    flags.push("group");
  }

  if (!account.isActive) {
    flags.push("inactive");
  }

  return flags.length > 0
    ? `${account.code} - ${account.name} (${flags.join(", ")})`
    : `${account.code} - ${account.name}`;
}
