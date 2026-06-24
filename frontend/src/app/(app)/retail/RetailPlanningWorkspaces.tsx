"use client";

import { FormEvent, useEffect, useMemo, useState } from "react";
import { Button, Card, Input, SecondaryButton, Table } from "@/components/ui";
import { apiGet, apiPost, apiPostNoContent } from "@/lib/api-client";

type ItemDto = { id: string; sku: string; name: string; barcode?: string | null; categoryId?: string | null };
type WarehouseDto = { id: string; code: string; name: string };
type PackDto = {
  id: string;
  itemId: string;
  code: string;
  name: string;
  barcode: string;
  baseQuantity: number;
  unitOfMeasure: string;
  price: number;
  purchaseAllowed: boolean;
  saleAllowed: boolean;
  transferAllowed: boolean;
  isDefaultPurchasePack: boolean;
  isDefaultSalesPack: boolean;
  isActive: boolean;
};
type BundleDto = {
  id: string;
  sku: string;
  name: string;
  barcode: string;
  type: number;
  price: number;
  isActive: boolean;
  components: { id: string; itemId: string; quantity: number; unitOfMeasure: string; isOptional: boolean }[];
};
type PromotionDto = {
  id: string;
  code: string;
  name: string;
  type: number;
  startsAt: string;
  endsAt: string;
  priority: number;
  isStackable: boolean;
  status: number;
  lines: unknown[];
};
type ReplenishmentRecommendation = {
  warehouseId: string;
  itemId: string;
  onHand: number;
  openPurchaseQuantity: number;
  reorderPoint: number;
  reorderQuantity: number;
  recommendedQuantity: number;
  status: number;
  preferredPackId?: string | null;
  recommendedPackQuantity?: number | null;
};
type ExpiryLayer = {
  id: string;
  warehouseId: string;
  itemId: string;
  remainingQuantity: number;
  unitCost: number;
  batchNumber?: string | null;
  expiryDate?: string | null;
};

const promotionTypeLabels: Record<number, string> = {
  1: "Percentage",
  2: "Fixed amount",
  3: "Special price",
  4: "Buy X get Y",
  5: "Bundle price",
  6: "Category",
  7: "Brand",
  8: "Quantity break",
};

const promotionStatusLabels: Record<number, string> = {
  1: "Draft",
  2: "Pending approval",
  3: "Approved",
  4: "Active",
  5: "Paused",
  6: "Expired",
  7: "Cancelled",
};

function itemLabel(items: ItemDto[], id: string): string {
  const item = items.find((x) => x.id === id);
  return item ? `${item.sku} - ${item.name}` : id;
}

function money(value: number): string {
  return new Intl.NumberFormat("en-LK", { style: "currency", currency: "LKR", maximumFractionDigits: 2 }).format(value);
}

function ErrorText({ message }: { message: string | null }) {
  if (!message) return null;
  return <div className="rounded-md border border-red-200 bg-red-50 px-3 py-2 text-sm text-red-700">{message}</div>;
}

function ItemSelect({ items, value, onChange, required = true }: { items: ItemDto[]; value: string; onChange: (value: string) => void; required?: boolean }) {
  return (
    <select value={value} onChange={(event) => onChange(event.target.value)} required={required} className="w-full rounded-md border border-[var(--input-border)] bg-[var(--surface)] px-2.5 py-1.5 text-[13px] shadow-[var(--shadow-control)]">
      <option value="">Select item</option>
      {items.map((item) => (
        <option key={item.id} value={item.id}>{item.sku} - {item.name}</option>
      ))}
    </select>
  );
}

function WarehouseSelect({ warehouses, value, onChange }: { warehouses: WarehouseDto[]; value: string; onChange: (value: string) => void }) {
  return (
    <select value={value} onChange={(event) => onChange(event.target.value)} required className="w-full rounded-md border border-[var(--input-border)] bg-[var(--surface)] px-2.5 py-1.5 text-[13px] shadow-[var(--shadow-control)]">
      <option value="">Select warehouse</option>
      {warehouses.map((warehouse) => (
        <option key={warehouse.id} value={warehouse.id}>{warehouse.code} - {warehouse.name}</option>
      ))}
    </select>
  );
}

export function PacksWorkspace() {
  const [items, setItems] = useState<ItemDto[]>([]);
  const [packs, setPacks] = useState<PackDto[]>([]);
  const [busy, setBusy] = useState(false);
  const [error, setError] = useState<string | null>(null);
  const [form, setForm] = useState({
    itemId: "",
    code: "",
    name: "",
    barcode: "",
    baseQuantity: "1",
    unitOfMeasure: "EA",
    price: "0",
    purchaseAllowed: true,
    saleAllowed: true,
    transferAllowed: true,
    isDefaultPurchasePack: false,
    isDefaultSalesPack: false,
  });

  async function load() {
    const [nextItems, nextPacks] = await Promise.all([apiGet<ItemDto[]>("items"), apiGet<PackDto[]>("retail/packs")]);
    setItems(nextItems);
    setPacks(nextPacks);
  }

  useEffect(() => {
    load().catch((err) => setError(err instanceof Error ? err.message : "Unable to load packs."));
  }, []);

  async function submit(event: FormEvent<HTMLFormElement>) {
    event.preventDefault();
    setBusy(true);
    setError(null);
    try {
      await apiPost<PackDto>("retail/packs", {
        ...form,
        baseQuantity: Number(form.baseQuantity),
        price: Number(form.price),
        taxCodeId: null,
        isActive: true,
      });
      setForm((current) => ({ ...current, code: "", name: "", barcode: "", baseQuantity: "1", price: "0" }));
      await load();
    } catch (err) {
      setError(err instanceof Error ? err.message : "Unable to save pack.");
    } finally {
      setBusy(false);
    }
  }

  return (
    <div className="space-y-5">
      <Card>
        <div className="text-lg font-semibold">Pack Items</div>
        <form className="mt-4 grid gap-3 md:grid-cols-4" onSubmit={submit}>
          <label className="space-y-1 text-sm md:col-span-2"><span className="font-medium">Item</span><ItemSelect items={items} value={form.itemId} onChange={(itemId) => setForm({ ...form, itemId })} /></label>
          <label className="space-y-1 text-sm"><span className="font-medium">Pack code</span><Input value={form.code} onChange={(e) => setForm({ ...form, code: e.target.value })} required /></label>
          <label className="space-y-1 text-sm"><span className="font-medium">Barcode</span><Input value={form.barcode} onChange={(e) => setForm({ ...form, barcode: e.target.value })} required /></label>
          <label className="space-y-1 text-sm md:col-span-2"><span className="font-medium">Pack name</span><Input value={form.name} onChange={(e) => setForm({ ...form, name: e.target.value })} required /></label>
          <label className="space-y-1 text-sm"><span className="font-medium">Base qty</span><Input type="number" min="0.0001" step="0.0001" value={form.baseQuantity} onChange={(e) => setForm({ ...form, baseQuantity: e.target.value })} required /></label>
          <label className="space-y-1 text-sm"><span className="font-medium">Price</span><Input type="number" min="0" step="0.01" value={form.price} onChange={(e) => setForm({ ...form, price: e.target.value })} required /></label>
          <div className="flex flex-wrap items-center gap-4 text-sm md:col-span-4">
            {(["purchaseAllowed", "saleAllowed", "transferAllowed", "isDefaultPurchasePack", "isDefaultSalesPack"] as const).map((key) => (
              <label key={key} className="inline-flex items-center gap-2"><input type="checkbox" checked={form[key]} onChange={(e) => setForm({ ...form, [key]: e.target.checked })} />{key.replace(/([A-Z])/g, " $1")}</label>
            ))}
          </div>
          <div className="md:col-span-4"><Button disabled={busy}>Create Pack</Button></div>
        </form>
      </Card>
      <ErrorText message={error} />
      <Card className="overflow-x-auto">
        <Table>
          <thead><tr><th>Item</th><th>Code</th><th>Barcode</th><th>Base Qty</th><th>Price</th><th>Flags</th></tr></thead>
          <tbody>{packs.map((pack) => (
            <tr key={pack.id}><td>{itemLabel(items, pack.itemId)}</td><td>{pack.code}</td><td>{pack.barcode}</td><td>{pack.baseQuantity} {pack.unitOfMeasure}</td><td>{money(pack.price)}</td><td>{[pack.purchaseAllowed && "Buy", pack.saleAllowed && "Sell", pack.isDefaultPurchasePack && "Default PO", pack.isDefaultSalesPack && "Default Sale"].filter(Boolean).join(", ")}</td></tr>
          ))}</tbody>
        </Table>
      </Card>
    </div>
  );
}

export function BundlesWorkspace() {
  const [items, setItems] = useState<ItemDto[]>([]);
  const [warehouses, setWarehouses] = useState<WarehouseDto[]>([]);
  const [bundles, setBundles] = useState<BundleDto[]>([]);
  const [warehouseId, setWarehouseId] = useState("");
  const [availability, setAvailability] = useState<Record<string, number>>({});
  const [error, setError] = useState<string | null>(null);
  const [form, setForm] = useState({ sku: "", name: "", barcode: "", type: "1", price: "0", itemId: "", quantity: "1" });

  async function load() {
    const [nextItems, nextWarehouses, nextBundles] = await Promise.all([apiGet<ItemDto[]>("items"), apiGet<WarehouseDto[]>("warehouses"), apiGet<BundleDto[]>("retail/bundles")]);
    setItems(nextItems);
    setWarehouses(nextWarehouses);
    setBundles(nextBundles);
  }

  useEffect(() => {
    const handle = window.setTimeout(() => {
      load().catch((err) => setError(err instanceof Error ? err.message : "Unable to load bundles."));
    }, 0);

    return () => window.clearTimeout(handle);
  }, []);

  async function submit(event: FormEvent<HTMLFormElement>) {
    event.preventDefault();
    setError(null);
    try {
      await apiPost<BundleDto>("retail/bundles", {
        sku: form.sku,
        name: form.name,
        barcode: form.barcode,
        type: Number(form.type),
        price: Number(form.price),
        startsOn: null,
        endsOn: null,
        categoryId: null,
        taxCodeId: null,
        isActive: true,
        components: [{ itemId: form.itemId, quantity: Number(form.quantity), unitOfMeasure: "EA", isOptional: false }],
      });
      setForm({ sku: "", name: "", barcode: "", type: "1", price: "0", itemId: "", quantity: "1" });
      await load();
    } catch (err) {
      setError(err instanceof Error ? err.message : "Unable to save bundle.");
    }
  }

  async function checkAvailability(id: string) {
    if (!warehouseId) return;
    const result = await apiGet<{ bundleId: string; availableQuantity: number }>(`retail/bundles/${id}/availability?warehouseId=${warehouseId}`);
    setAvailability((current) => ({ ...current, [id]: result.availableQuantity }));
  }

  return (
    <div className="space-y-5">
      <Card>
        <div className="text-lg font-semibold">Bundle Items</div>
        <form className="mt-4 grid gap-3 md:grid-cols-4" onSubmit={submit}>
          <label className="space-y-1 text-sm"><span className="font-medium">SKU</span><Input value={form.sku} onChange={(e) => setForm({ ...form, sku: e.target.value })} required /></label>
          <label className="space-y-1 text-sm"><span className="font-medium">Barcode</span><Input value={form.barcode} onChange={(e) => setForm({ ...form, barcode: e.target.value })} required /></label>
          <label className="space-y-1 text-sm md:col-span-2"><span className="font-medium">Name</span><Input value={form.name} onChange={(e) => setForm({ ...form, name: e.target.value })} required /></label>
          <label className="space-y-1 text-sm"><span className="font-medium">Price</span><Input type="number" min="0" step="0.01" value={form.price} onChange={(e) => setForm({ ...form, price: e.target.value })} required /></label>
          <label className="space-y-1 text-sm md:col-span-2"><span className="font-medium">First component</span><ItemSelect items={items} value={form.itemId} onChange={(itemId) => setForm({ ...form, itemId })} /></label>
          <label className="space-y-1 text-sm"><span className="font-medium">Component qty</span><Input type="number" min="0.0001" step="0.0001" value={form.quantity} onChange={(e) => setForm({ ...form, quantity: e.target.value })} required /></label>
          <div className="md:col-span-4"><Button>Create Bundle</Button></div>
        </form>
      </Card>
      <ErrorText message={error} />
      <Card className="overflow-x-auto">
        <div className="mb-3 max-w-sm"><WarehouseSelect warehouses={warehouses} value={warehouseId} onChange={setWarehouseId} /></div>
        <Table>
          <thead><tr><th>SKU</th><th>Name</th><th>Barcode</th><th>Price</th><th>Components</th><th>Availability</th><th></th></tr></thead>
          <tbody>{bundles.map((bundle) => (
            <tr key={bundle.id}><td>{bundle.sku}</td><td>{bundle.name}</td><td>{bundle.barcode}</td><td>{money(bundle.price)}</td><td>{bundle.components.map((c) => `${itemLabel(items, c.itemId)} x ${c.quantity}`).join(", ")}</td><td>{availability[bundle.id] ?? "-"}</td><td><SecondaryButton type="button" onClick={() => checkAvailability(bundle.id)}>Check</SecondaryButton></td></tr>
          ))}</tbody>
        </Table>
      </Card>
    </div>
  );
}

export function PromotionsWorkspace() {
  const [items, setItems] = useState<ItemDto[]>([]);
  const [packs, setPacks] = useState<PackDto[]>([]);
  const [promotions, setPromotions] = useState<PromotionDto[]>([]);
  const [error, setError] = useState<string | null>(null);
  const [form, setForm] = useState({ code: "", name: "", type: "1", startsAt: "", endsAt: "", itemId: "", itemPackId: "", discountPercent: "0", specialPrice: "" });

  async function load() {
    const [nextItems, nextPacks, nextPromotions] = await Promise.all([apiGet<ItemDto[]>("items"), apiGet<PackDto[]>("retail/packs"), apiGet<PromotionDto[]>("retail/promotions")]);
    setItems(nextItems);
    setPacks(nextPacks);
    setPromotions(nextPromotions);
  }

  useEffect(() => {
    const handle = window.setTimeout(() => {
      load().catch((err) => setError(err instanceof Error ? err.message : "Unable to load promotions."));
    }, 0);

    return () => window.clearTimeout(handle);
  }, []);

  async function submit(event: FormEvent<HTMLFormElement>) {
    event.preventDefault();
    setError(null);
    try {
      await apiPost<PromotionDto>("retail/promotions", {
        code: form.code,
        name: form.name,
        description: null,
        type: Number(form.type),
        startsAt: new Date(form.startsAt).toISOString(),
        endsAt: new Date(form.endsAt).toISOString(),
        priority: 100,
        isStackable: false,
        maxDiscountAmount: null,
        lines: [{
          itemId: form.itemId || null,
          itemPackId: form.itemPackId || null,
          bundleId: null,
          categoryId: null,
          brandId: null,
          buyQuantity: null,
          getItemId: null,
          getQuantity: null,
          discountPercent: form.discountPercent ? Number(form.discountPercent) : null,
          discountAmount: null,
          specialPrice: form.specialPrice ? Number(form.specialPrice) : null,
          minimumBasketAmount: null,
        }],
      });
      setForm({ code: "", name: "", type: "1", startsAt: "", endsAt: "", itemId: "", itemPackId: "", discountPercent: "0", specialPrice: "" });
      await load();
    } catch (err) {
      setError(err instanceof Error ? err.message : "Unable to save promotion.");
    }
  }

  async function action(id: string, verb: "submit" | "approve" | "activate" | "pause") {
    await apiPostNoContent(`retail/promotions/${id}/${verb}`, {});
    await load();
  }

  return (
    <div className="space-y-5">
      <Card>
        <div className="text-lg font-semibold">Promotions</div>
        <form className="mt-4 grid gap-3 md:grid-cols-4" onSubmit={submit}>
          <label className="space-y-1 text-sm"><span className="font-medium">Code</span><Input value={form.code} onChange={(e) => setForm({ ...form, code: e.target.value })} required /></label>
          <label className="space-y-1 text-sm"><span className="font-medium">Name</span><Input value={form.name} onChange={(e) => setForm({ ...form, name: e.target.value })} required /></label>
          <label className="space-y-1 text-sm"><span className="font-medium">Starts</span><Input type="datetime-local" value={form.startsAt} onChange={(e) => setForm({ ...form, startsAt: e.target.value })} required /></label>
          <label className="space-y-1 text-sm"><span className="font-medium">Ends</span><Input type="datetime-local" value={form.endsAt} onChange={(e) => setForm({ ...form, endsAt: e.target.value })} required /></label>
          <label className="space-y-1 text-sm"><span className="font-medium">Type</span><select value={form.type} onChange={(e) => setForm({ ...form, type: e.target.value })} className="w-full rounded-md border border-[var(--input-border)] bg-[var(--surface)] px-2.5 py-1.5 text-[13px] shadow-[var(--shadow-control)]">{Object.entries(promotionTypeLabels).map(([value, label]) => <option key={value} value={value}>{label}</option>)}</select></label>
          <label className="space-y-1 text-sm md:col-span-2"><span className="font-medium">Item</span><ItemSelect items={items} value={form.itemId} onChange={(itemId) => setForm({ ...form, itemId, itemPackId: "" })} required={false} /></label>
          <label className="space-y-1 text-sm"><span className="font-medium">Pack</span><select value={form.itemPackId} onChange={(e) => setForm({ ...form, itemPackId: e.target.value, itemId: "" })} className="w-full rounded-md border border-[var(--input-border)] bg-[var(--surface)] px-2.5 py-1.5 text-[13px] shadow-[var(--shadow-control)]"><option value="">Any pack</option>{packs.map((pack) => <option key={pack.id} value={pack.id}>{pack.code} - {pack.name}</option>)}</select></label>
          <label className="space-y-1 text-sm"><span className="font-medium">Discount %</span><Input type="number" min="0" step="0.01" value={form.discountPercent} onChange={(e) => setForm({ ...form, discountPercent: e.target.value })} /></label>
          <label className="space-y-1 text-sm"><span className="font-medium">Special price</span><Input type="number" min="0" step="0.01" value={form.specialPrice} onChange={(e) => setForm({ ...form, specialPrice: e.target.value })} /></label>
          <div className="md:col-span-4"><Button>Create Promotion</Button></div>
        </form>
      </Card>
      <ErrorText message={error} />
      <Card className="overflow-x-auto">
        <Table><thead><tr><th>Code</th><th>Name</th><th>Type</th><th>Window</th><th>Status</th><th>Actions</th></tr></thead>
          <tbody>{promotions.map((promo) => <tr key={promo.id}><td>{promo.code}</td><td>{promo.name}</td><td>{promotionTypeLabels[promo.type]}</td><td>{new Date(promo.startsAt).toLocaleString()} - {new Date(promo.endsAt).toLocaleString()}</td><td>{promotionStatusLabels[promo.status]}</td><td className="space-x-2"><SecondaryButton type="button" onClick={() => action(promo.id, "submit")}>Submit</SecondaryButton><SecondaryButton type="button" onClick={() => action(promo.id, "approve")}>Approve</SecondaryButton><SecondaryButton type="button" onClick={() => action(promo.id, "activate")}>Activate</SecondaryButton><SecondaryButton type="button" onClick={() => action(promo.id, "pause")}>Pause</SecondaryButton></td></tr>)}</tbody>
        </Table>
      </Card>
    </div>
  );
}

export function ReplenishmentWorkspace() {
  const [warehouses, setWarehouses] = useState<WarehouseDto[]>([]);
  const [items, setItems] = useState<ItemDto[]>([]);
  const [warehouseId, setWarehouseId] = useState("");
  const [rows, setRows] = useState<ReplenishmentRecommendation[]>([]);
  const [error, setError] = useState<string | null>(null);

  useEffect(() => {
    Promise.all([apiGet<WarehouseDto[]>("warehouses"), apiGet<ItemDto[]>("items")])
      .then(([nextWarehouses, nextItems]) => { setWarehouses(nextWarehouses); setItems(nextItems); })
      .catch((err) => setError(err instanceof Error ? err.message : "Unable to load data."));
  }, []);

  async function calculate() {
    setError(null);
    setRows(await apiGet<ReplenishmentRecommendation[]>(`replenishment/recommendations?warehouseId=${warehouseId}`));
  }

  async function createRun() {
    const result = await apiPost<{ id: string }>("replenishment/runs", { warehouseId, notes: "Created from replenishment workbench." });
    await apiPost<{ purchaseRequisitionId: string }>(`replenishment/runs/${result.id}/create-purchase-requisition`, { submit: false });
    await calculate();
  }

  const totalRecommended = useMemo(() => rows.reduce((sum, row) => sum + row.recommendedQuantity, 0), [rows]);

  return (
    <div className="space-y-5">
      <Card>
        <div className="text-lg font-semibold">Item Replenishment</div>
        <div className="mt-4 flex flex-wrap items-end gap-3">
          <label className="w-full max-w-sm space-y-1 text-sm"><span className="font-medium">Warehouse</span><WarehouseSelect warehouses={warehouses} value={warehouseId} onChange={setWarehouseId} /></label>
          <Button type="button" onClick={calculate} disabled={!warehouseId}>Calculate</Button>
          <SecondaryButton type="button" onClick={createRun} disabled={!warehouseId || totalRecommended <= 0}>Create PR</SecondaryButton>
        </div>
      </Card>
      <ErrorText message={error} />
      <Card className="overflow-x-auto">
        <Table><thead><tr><th>Item</th><th>On Hand</th><th>Open PO</th><th>Reorder Point</th><th>Configured Qty</th><th>Recommended</th><th>Pack Qty</th></tr></thead>
          <tbody>{rows.map((row) => <tr key={row.itemId}><td>{itemLabel(items, row.itemId)}</td><td>{row.onHand}</td><td>{row.openPurchaseQuantity}</td><td>{row.reorderPoint}</td><td>{row.reorderQuantity}</td><td className={row.recommendedQuantity > 0 ? "font-semibold text-amber-700" : ""}>{row.recommendedQuantity}</td><td>{row.recommendedPackQuantity ?? "-"}</td></tr>)}</tbody>
        </Table>
      </Card>
    </div>
  );
}

export function ExpiryWorkspace() {
  const [warehouses, setWarehouses] = useState<WarehouseDto[]>([]);
  const [items, setItems] = useState<ItemDto[]>([]);
  const [warehouseId, setWarehouseId] = useState("");
  const [nearExpiry, setNearExpiry] = useState<ExpiryLayer[]>([]);
  const [expired, setExpired] = useState<ExpiryLayer[]>([]);
  const [error, setError] = useState<string | null>(null);

  useEffect(() => {
    Promise.all([apiGet<WarehouseDto[]>("warehouses"), apiGet<ItemDto[]>("items")])
      .then(([nextWarehouses, nextItems]) => { setWarehouses(nextWarehouses); setItems(nextItems); })
      .catch((err) => setError(err instanceof Error ? err.message : "Unable to load data."));
  }, []);

  async function load() {
    setError(null);
    const query = warehouseId ? `?warehouseId=${warehouseId}` : "";
    const [near, exp] = await Promise.all([apiGet<ExpiryLayer[]>(`inventory/expiry/near-expiry${warehouseId ? `${query}&days=30` : "?days=30"}`), apiGet<ExpiryLayer[]>(`inventory/expiry/expired${query}`)]);
    setNearExpiry(near);
    setExpired(exp);
  }

  async function writeOff(layer: ExpiryLayer) {
    const result = await apiPost<{ id: string }>("inventory/expiry/write-offs", { warehouseId: layer.warehouseId, notes: "Expired stock write-off.", lines: [{ stockLayerId: layer.id, quantity: layer.remainingQuantity, reason: "Expired stock" }] });
    await apiPostNoContent(`inventory/expiry/write-offs/${result.id}/approve`, {});
    await apiPostNoContent(`inventory/expiry/write-offs/${result.id}/post`, {});
    await load();
  }

  function tableRows(rows: ExpiryLayer[], allowWriteOff: boolean) {
    return rows.map((row) => <tr key={row.id}><td>{itemLabel(items, row.itemId)}</td><td>{row.batchNumber ?? "-"}</td><td>{row.expiryDate ?? "-"}</td><td>{row.remainingQuantity}</td><td>{money(row.unitCost)}</td><td>{allowWriteOff ? <SecondaryButton type="button" onClick={() => writeOff(row)}>Write Off</SecondaryButton> : null}</td></tr>);
  }

  return (
    <div className="space-y-5">
      <Card>
        <div className="text-lg font-semibold">Expiry Handling</div>
        <div className="mt-4 flex flex-wrap items-end gap-3">
          <label className="w-full max-w-sm space-y-1 text-sm"><span className="font-medium">Warehouse</span><WarehouseSelect warehouses={warehouses} value={warehouseId} onChange={setWarehouseId} /></label>
          <Button type="button" onClick={load}>Load Expiry Stock</Button>
        </div>
      </Card>
      <ErrorText message={error} />
      <Card className="overflow-x-auto"><div className="mb-2 font-semibold">Expired Stock</div><Table><thead><tr><th>Item</th><th>Batch</th><th>Expiry</th><th>Qty</th><th>Unit Cost</th><th></th></tr></thead><tbody>{tableRows(expired, true)}</tbody></Table></Card>
      <Card className="overflow-x-auto"><div className="mb-2 font-semibold">Near Expiry Stock</div><Table><thead><tr><th>Item</th><th>Batch</th><th>Expiry</th><th>Qty</th><th>Unit Cost</th><th></th></tr></thead><tbody>{tableRows(nearExpiry, false)}</tbody></Table></Card>
    </div>
  );
}
