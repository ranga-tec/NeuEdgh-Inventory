"use client";

import { FormEvent, useEffect, useMemo, useState } from "react";
import { Button, Card, Input, SecondaryButton, Table, Textarea } from "@/components/ui";
import { apiGet, apiPost, apiPostNoContent } from "@/lib/api-client";

type UtilityPaymentType = 1 | 2;
type UtilityPaymentStatus = 1 | 2 | 3;

type UtilityPaymentDto = {
  id: string;
  number: string;
  type: UtilityPaymentType;
  status: UtilityPaymentStatus;
  provider: string;
  accountNumber: string;
  customerPhone?: string | null;
  amount: number;
  serviceFee: number;
  total: number;
  paymentTypeId?: string | null;
  paymentTypeName?: string | null;
  externalReference?: string | null;
  notes?: string | null;
  createdAt: string;
  postedAt?: string | null;
  voidedAt?: string | null;
  voidReason?: string | null;
};

const typeLabels: Record<UtilityPaymentType, string> = {
  1: "Airtime",
  2: "Bill payment",
};

const statusLabels: Record<UtilityPaymentStatus, string> = {
  1: "Draft",
  2: "Posted",
  3: "Voided",
};

const commonProviders = ["Dialog", "Mobitel", "Airtel", "Hutch", "SLT", "CEB", "Water Board", "Insurance"];

function money(value: number): string {
  return new Intl.NumberFormat("en-LK", {
    style: "currency",
    currency: "LKR",
    maximumFractionDigits: 2,
  }).format(value);
}

function dateTime(value: string): string {
  return new Intl.DateTimeFormat("en-LK", {
    dateStyle: "medium",
    timeStyle: "short",
  }).format(new Date(value));
}

export function UtilityPaymentsWorkspace() {
  const [payments, setPayments] = useState<UtilityPaymentDto[]>([]);
  const [type, setType] = useState<UtilityPaymentType>(1);
  const [provider, setProvider] = useState("Dialog");
  const [accountNumber, setAccountNumber] = useState("");
  const [customerPhone, setCustomerPhone] = useState("");
  const [amount, setAmount] = useState("");
  const [serviceFee, setServiceFee] = useState("0");
  const [externalReference, setExternalReference] = useState("");
  const [notes, setNotes] = useState("");
  const [busy, setBusy] = useState(false);
  const [error, setError] = useState<string | null>(null);

  async function loadPayments() {
    setPayments(await apiGet<UtilityPaymentDto[]>("retail/utility-payments"));
  }

  useEffect(() => {
    loadPayments().catch((err: unknown) => setError(err instanceof Error ? err.message : "Unable to load utility payments."));
  }, []);

  const totals = useMemo(() => {
    return payments.reduce(
      (current, payment) => {
        if (payment.status === 2) {
          current.posted += payment.total;
        }
        if (payment.status === 1) {
          current.drafts += 1;
        }
        return current;
      },
      { posted: 0, drafts: 0 },
    );
  }, [payments]);

  async function submit(event: FormEvent<HTMLFormElement>) {
    event.preventDefault();
    setBusy(true);
    setError(null);

    try {
      await apiPost<UtilityPaymentDto>("retail/utility-payments", {
        type,
        provider,
        accountNumber,
        customerPhone: customerPhone || null,
        amount: Number(amount),
        serviceFee: Number(serviceFee || 0),
        externalReference: externalReference || null,
        notes: notes || null,
      });
      setAccountNumber("");
      setCustomerPhone("");
      setAmount("");
      setServiceFee("0");
      setExternalReference("");
      setNotes("");
      await loadPayments();
    } catch (err) {
      setError(err instanceof Error ? err.message : "Unable to create utility payment.");
    } finally {
      setBusy(false);
    }
  }

  async function postPayment(id: string) {
    setBusy(true);
    setError(null);
    try {
      await apiPostNoContent(`retail/utility-payments/${id}/post`, {});
      await loadPayments();
    } catch (err) {
      setError(err instanceof Error ? err.message : "Unable to post utility payment.");
    } finally {
      setBusy(false);
    }
  }

  async function voidPayment(id: string) {
    const reason = window.prompt("Reason for voiding this utility payment");
    if (!reason) return;

    setBusy(true);
    setError(null);
    try {
      await apiPostNoContent(`retail/utility-payments/${id}/void`, { reason });
      await loadPayments();
    } catch (err) {
      setError(err instanceof Error ? err.message : "Unable to void utility payment.");
    } finally {
      setBusy(false);
    }
  }

  return (
    <div className="space-y-6">
      <div className="grid gap-4 xl:grid-cols-[minmax(0,0.9fr)_minmax(0,1.5fr)]">
        <Card>
          <div className="text-lg font-semibold text-[var(--foreground)]">New Utility Payment</div>
          <p className="mt-1 text-sm text-[var(--muted-foreground)]">
            Record airtime reloads and customer bill payments collected at the counter.
          </p>

          <form className="mt-5 space-y-4" onSubmit={submit}>
            <div className="grid gap-3 sm:grid-cols-2">
              <label className="space-y-1 text-sm">
                <span className="font-medium">Type</span>
                <select
                  value={type}
                  onChange={(event) => setType(Number(event.target.value) as UtilityPaymentType)}
                  className="w-full rounded-md border border-[var(--input-border)] bg-[var(--surface)] px-2.5 py-1.5 text-[13px] shadow-[var(--shadow-control)]"
                >
                  <option value={1}>Airtime</option>
                  <option value={2}>Bill payment</option>
                </select>
              </label>

              <label className="space-y-1 text-sm">
                <span className="font-medium">Provider</span>
                <input
                  list="utility-providers"
                  value={provider}
                  onChange={(event) => setProvider(event.target.value)}
                  className="w-full rounded-md border border-[var(--input-border)] bg-[var(--surface)] px-2.5 py-1.5 text-[13px] shadow-[var(--shadow-control)]"
                  required
                />
                <datalist id="utility-providers">
                  {commonProviders.map((name) => (
                    <option key={name} value={name} />
                  ))}
                </datalist>
              </label>
            </div>

            <label className="space-y-1 text-sm">
              <span className="font-medium">{type === 1 ? "Mobile number" : "Bill/account number"}</span>
              <Input value={accountNumber} onChange={(event) => setAccountNumber(event.target.value)} required />
            </label>

            <label className="space-y-1 text-sm">
              <span className="font-medium">Customer phone</span>
              <Input value={customerPhone} onChange={(event) => setCustomerPhone(event.target.value)} />
            </label>

            <div className="grid gap-3 sm:grid-cols-2">
              <label className="space-y-1 text-sm">
                <span className="font-medium">Amount</span>
                <Input type="number" min="0.01" step="0.01" value={amount} onChange={(event) => setAmount(event.target.value)} required />
              </label>
              <label className="space-y-1 text-sm">
                <span className="font-medium">Service fee</span>
                <Input type="number" min="0" step="0.01" value={serviceFee} onChange={(event) => setServiceFee(event.target.value)} />
              </label>
            </div>

            <label className="space-y-1 text-sm">
              <span className="font-medium">External reference</span>
              <Input value={externalReference} onChange={(event) => setExternalReference(event.target.value)} />
            </label>

            <label className="space-y-1 text-sm">
              <span className="font-medium">Notes</span>
              <Textarea value={notes} onChange={(event) => setNotes(event.target.value)} />
            </label>

            {error ? <div className="rounded-md border border-rose-200 bg-rose-50 px-3 py-2 text-sm text-rose-700">{error}</div> : null}

            <Button type="submit" disabled={busy}>
              Save utility payment
            </Button>
          </form>
        </Card>

        <div className="space-y-4">
          <div className="grid gap-3 sm:grid-cols-2">
            <Card>
              <div className="text-xs font-semibold uppercase tracking-[0.2em] text-[var(--muted-foreground)]">Posted today</div>
              <div className="mt-2 text-2xl font-semibold">{money(totals.posted)}</div>
            </Card>
            <Card>
              <div className="text-xs font-semibold uppercase tracking-[0.2em] text-[var(--muted-foreground)]">Drafts</div>
              <div className="mt-2 text-2xl font-semibold">{totals.drafts}</div>
            </Card>
          </div>

          <Card className="overflow-x-auto">
            <div className="mb-3 flex items-center justify-between gap-3">
              <div>
                <div className="text-lg font-semibold">Recent Transactions</div>
                <p className="text-sm text-[var(--muted-foreground)]">Post a draft after the customer has paid.</p>
              </div>
              <SecondaryButton type="button" onClick={() => loadPayments()} disabled={busy}>
                Refresh
              </SecondaryButton>
            </div>

            <Table>
              <thead>
                <tr>
                  <th>Number</th>
                  <th>Type</th>
                  <th>Provider</th>
                  <th>Account</th>
                  <th>Total</th>
                  <th>Status</th>
                  <th>Created</th>
                  <th>Actions</th>
                </tr>
              </thead>
              <tbody>
                {payments.map((payment) => (
                  <tr key={payment.id}>
                    <td>{payment.number}</td>
                    <td>{typeLabels[payment.type]}</td>
                    <td>{payment.provider}</td>
                    <td>{payment.accountNumber}</td>
                    <td>{money(payment.total)}</td>
                    <td>{statusLabels[payment.status]}</td>
                    <td>{dateTime(payment.createdAt)}</td>
                    <td>
                      <div className="flex flex-wrap gap-2">
                        {payment.status === 1 ? (
                          <>
                            <SecondaryButton type="button" onClick={() => postPayment(payment.id)} disabled={busy}>
                              Post
                            </SecondaryButton>
                            <SecondaryButton type="button" onClick={() => voidPayment(payment.id)} disabled={busy}>
                              Void
                            </SecondaryButton>
                          </>
                        ) : (
                          <span className="text-sm text-[var(--muted-foreground)]">No actions</span>
                        )}
                      </div>
                    </td>
                  </tr>
                ))}
                {payments.length === 0 ? (
                  <tr>
                    <td colSpan={8} className="text-center text-[var(--muted-foreground)]">
                      No utility payments recorded yet.
                    </td>
                  </tr>
                ) : null}
              </tbody>
            </Table>
          </Card>
        </div>
      </div>
    </div>
  );
}
